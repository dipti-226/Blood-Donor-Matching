import { Component, inject, input, output, signal } from '@angular/core';
import { DonorApi } from '../../../core/donor/donor-api.service';
import { ToastService } from '../../../core/ui/toast.service';
import { errorText } from '../../../shared/utils/error-text';

// The "available for requests" switch. It saves straight away; the parent keeps the current value.
@Component({
  selector: 'app-availability-card',
  templateUrl: './availability-card.html',
  styleUrl: './availability-card.css',
})
export class AvailabilityCard {
  private readonly api = inject(DonorApi);
  private readonly toast = inject(ToastService);

  readonly available = input.required<boolean>();
  readonly disabled = input<boolean>(false);
  readonly availableChange = output<boolean>();

  protected readonly busy = signal(false);

  protected toggle(event: Event): void {
    const control = event.target as HTMLInputElement;
    const next = control.checked;

    if (this.busy()) {
      control.checked = !next;
      return;
    }

    this.busy.set(true);

    this.api.setAvailability(next).subscribe({
      next: (result) => {
        this.busy.set(false);
        this.availableChange.emit(result.isAvailable);
        this.toast.success(
          result.isAvailable ? 'You are now available for requests.' : 'You will not be matched to new requests.',
        );
      },
      error: (err: unknown) => {
        this.busy.set(false);
        control.checked = !next;
        this.toast.error(errorText(err));
      },
    });
  }
}