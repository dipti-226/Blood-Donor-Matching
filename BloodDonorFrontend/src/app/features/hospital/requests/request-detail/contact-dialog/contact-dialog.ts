import { Component, OnDestroy, inject, input, output, signal } from '@angular/core';
import { HospitalApi } from '../../../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../../../core/http/api-error';
import { DonorContact, HospitalDonorRequest } from '../../../../../core/models/blood-request.models';
import { BloodGroupChip } from '../../../../../shared/ui/blood-group-chip/blood-group-chip';
import { ModalDialog } from '../../../../../shared/ui/modal-dialog/modal-dialog';

// Shows an accepted donor's name and number, but only after an explicit click, because the API
// records every view. The details live only in this dialog: they are never logged, stored or put
// in a URL, and they are dropped when the dialog closes.
@Component({
  selector: 'app-contact-dialog',
  imports: [ModalDialog, BloodGroupChip],
  templateUrl: './contact-dialog.html',
  styleUrl: './contact-dialog.css',
})
export class ContactDialog implements OnDestroy {
  private readonly api = inject(HospitalApi);

  readonly bloodRequestId = input.required<string>();
  readonly item = input.required<HospitalDonorRequest>();

  readonly closed = output<void>();
  // The donor request is no longer accepted or the blood request is closed. Carries the reason to show.
  readonly stale = output<string>();

  protected readonly contact = signal<DonorContact | null>(null);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnDestroy(): void {
    this.contact.set(null);
  }

  protected reveal(): void {
    if (this.busy() || this.contact()) {
      return;
    }

    this.busy.set(true);
    this.error.set(null);

    this.api.getDonorContact(this.bloodRequestId(), this.item().id).subscribe({
      next: (contact) => {
        this.busy.set(false);
        this.contact.set(contact);
      },
      error: (err: unknown) => {
        this.busy.set(false);

        if (err instanceof ApiError && (err.status === 404 || err.status === 409)) {
          this.stale.emit(err.message);
          return;
        }

        this.error.set(err instanceof Error ? err.message : 'Something went wrong. Please try again.');
      },
    });
  }
}