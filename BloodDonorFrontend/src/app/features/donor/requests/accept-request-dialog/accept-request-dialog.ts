import { Component, inject, input, output, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DonorApi } from '../../../../core/donor/donor-api.service';
import { ApiError } from '../../../../core/http/api-error';
import { DonorOwnRequest } from '../../../../core/models/donor.models';
import { ComponentLabelPipe } from '../../../../shared/pipes/component-label';
import { BloodGroupLabelPipe } from '../../../../shared/pipes/blood-group-label';
import { FormField } from '../../../../shared/ui/form-field/form-field';
import { ModalDialog } from '../../../../shared/ui/modal-dialog/modal-dialog';
import { applyServerErrors, errorMessage } from '../../../../shared/utils/form-errors';
import { phoneNumber } from '../../../../shared/utils/validators';

// Asks for the number to share and accepts the request. The number lives only in this dialog's form,
// which is destroyed when the dialog closes. It is never logged or stored by the browser.
@Component({
  selector: 'app-accept-request-dialog',
  imports: [ReactiveFormsModule, ModalDialog, FormField, ComponentLabelPipe, BloodGroupLabelPipe],
  templateUrl: './accept-request-dialog.html',
  styleUrl: './accept-request-dialog.css',
})
export class AcceptRequestDialog {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(DonorApi);

  readonly request = input.required<DonorOwnRequest>();

  // The request was accepted.
  readonly accepted = output<void>();
  // The donor closed the dialog without accepting.
  readonly cancelled = output<void>();
  // The request changed on the server (answered, cancelled or closed). Carries the reason to show.
  readonly stale = output<string>();

  protected readonly form = this.fb.group({
    contactNumber: ['', [Validators.required, phoneNumber]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected fieldError(): string | null {
    return errorMessage(this.form.controls.contactNumber);
  }

  protected submit(): void {
    if (this.busy()) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.error.set(null);

    const contactNumber = this.form.controls.contactNumber.value.trim();

    this.api.acceptRequest(this.request().donorRequestId, contactNumber).subscribe({
      next: () => {
        this.busy.set(false);
        this.accepted.emit();
      },
      error: (err: unknown) => this.onError(err),
    });
  }

  private onError(err: unknown): void {
    this.busy.set(false);

    if (!(err instanceof ApiError)) {
      this.error.set('Something went wrong. Please try again.');
      return;
    }

    if (err.status === 404 || err.status === 409) {
      this.stale.emit(err.message);
      return;
    }

    const unmatched = applyServerErrors(this.form, err);
    this.error.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
}