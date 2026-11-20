import { Component, inject, input, output, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HospitalApi } from '../../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../../core/http/api-error';
import { VerificationListItem } from '../../../../core/models/blood-request.models';
import { BLOOD_GROUP_LABELS } from '../../../../core/models/labels';
import { FormField } from '../../../../shared/ui/form-field/form-field';
import { ModalDialog } from '../../../../shared/ui/modal-dialog/modal-dialog';
import { applyServerErrors, errorMessage } from '../../../../shared/utils/form-errors';
import { notBlank } from '../../../../shared/utils/validators';

@Component({
  selector: 'app-reject-dialog',
  imports: [ReactiveFormsModule, ModalDialog, FormField],
  templateUrl: './reject-dialog.html',
  styleUrl: './reject-dialog.css',
})
export class RejectDialog {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(HospitalApi);

  readonly item = input.required<VerificationListItem>();

  readonly rejected = output<void>();
  readonly cancelled = output<void>();
  // The record changed on the server (already decided, or gone). Carries the reason to show.
  readonly stale = output<string>();

  protected readonly labels = BLOOD_GROUP_LABELS;

  protected readonly form = this.fb.group({
    reason: ['', [Validators.required, notBlank, Validators.maxLength(1000)]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  protected fieldError(): string | null {
    return errorMessage(this.form.controls.reason);
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

    this.api.rejectVerification(this.item().id, this.form.controls.reason.value.trim()).subscribe({
      next: () => {
        this.busy.set(false);
        this.rejected.emit();
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