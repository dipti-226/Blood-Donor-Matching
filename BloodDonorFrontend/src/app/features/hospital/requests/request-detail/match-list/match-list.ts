import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { HospitalApi } from '../../../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../../../core/http/api-error';
import { DonorMatchList } from '../../../../../core/models/blood-request.models';
import { ToastService } from '../../../../../core/ui/toast.service';
import { BloodGroupChip } from '../../../../../shared/ui/blood-group-chip/blood-group-chip';
import { EmptyState } from '../../../../../shared/ui/empty-state/empty-state';

// The API accepts at most 20 donors per send and 100 donor requests per blood request.
const MAX_PER_SEND = 20;
const MAX_PER_REQUEST = 100;

// Compatible donors for an open blood request. The API does not hide donors who were already asked,
// so they are marked here and cannot be picked again.
@Component({
  selector: 'app-match-list',
  imports: [BloodGroupChip, EmptyState],
  templateUrl: './match-list.html',
  styleUrl: './match-list.css',
})
export class MatchList {
  private readonly api = inject(HospitalApi);
  private readonly toast = inject(ToastService);

  readonly bloodRequestId = input.required<string>();
  readonly matchList = input.required<DonorMatchList>();
  // References of every donor who already has a donor request (any status).
  readonly askedReferences = input.required<ReadonlySet<string>>();
  readonly askedTotal = input.required<number>();

  // The donor requests or the matches changed on the server: the page should reload.
  readonly changed = output<void>();

  protected readonly selected = signal<ReadonlySet<string>>(new Set());
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly maxSelectable = computed(() =>
    Math.max(0, Math.min(MAX_PER_SEND, MAX_PER_REQUEST - this.askedTotal())),
  );
  protected readonly available = computed(() =>
    this.matchList().matches.filter((match) => !this.askedReferences().has(match.donorReference)),
  );
  protected readonly atLimit = computed(() => this.askedTotal() >= MAX_PER_REQUEST);

  constructor() {
    // Never keep a donor selected after they have been asked or have dropped out of the matches.
    effect(() => {
      const valid = new Set(this.available().map((match) => match.donorReference));
      const current = this.selected();
      const kept = [...current].filter((reference) => valid.has(reference));

      if (kept.length !== current.size) {
        this.selected.set(new Set(kept));
      }
    });
  }

  protected isAsked(reference: string): boolean {
    return this.askedReferences().has(reference);
  }

  protected isSelected(reference: string): boolean {
    return this.selected().has(reference);
  }

  protected isDisabled(reference: string): boolean {
    return (
      this.busy() ||
      this.isAsked(reference) ||
      (!this.isSelected(reference) && this.selected().size >= this.maxSelectable())
    );
  }

  protected toggle(reference: string): void {
    const next = new Set(this.selected());

    if (next.has(reference)) {
      next.delete(reference);
    } else if (next.size < this.maxSelectable()) {
      next.add(reference);
    }

    this.selected.set(next);
  }

  protected selectFirst(): void {
    this.selected.set(
      new Set(this.available().slice(0, this.maxSelectable()).map((match) => match.donorReference)),
    );
  }

  protected clear(): void {
    this.selected.set(new Set());
  }

  protected send(): void {
    const references = [...this.selected()];
    if (this.busy() || references.length === 0) {
      return;
    }

    this.busy.set(true);
    this.error.set(null);

    this.api.sendDonorRequests(this.bloodRequestId(), references).subscribe({
      next: () => {
        this.busy.set(false);
        this.selected.set(new Set());
        this.toast.success(references.length === 1 ? '1 donor was asked.' : `${references.length} donors were asked.`);
        this.changed.emit();
      },
      error: (err: unknown) => {
        this.busy.set(false);

        if (err instanceof ApiError) {
          this.error.set(err.message);
          // The matches or donor requests moved on (a donor was asked by a colleague, or is no longer a match).
          if (err.status === 400 || err.status === 404 || err.status === 409) {
            this.selected.set(new Set());
            this.changed.emit();
          }
        } else {
          this.error.set('Something went wrong. Please try again.');
        }
      },
    });
  }
}