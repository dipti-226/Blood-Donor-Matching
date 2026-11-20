import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HospitalApi } from '../../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../../core/http/api-error';
import { VerificationListItem } from '../../../../core/models/blood-request.models';
import { BloodGroup } from '../../../../core/models/enums';
import { BLOOD_GROUP_LABELS, BLOOD_GROUP_OPTIONS } from '../../../../core/models/labels';
import { FormField } from '../../../../shared/ui/form-field/form-field';
import { ModalDialog } from '../../../../shared/ui/modal-dialog/modal-dialog';
import { applyServerErrors, errorMessage } from '../../../../shared/utils/form-errors';

// The reviewer confirms the blood group. It starts as the declared group but the reviewer picks the final one.
@Component({
  selector: 'app-approve-dialog',
  imports: [ReactiveFormsModule, ModalDialog, FormField],
  templateUrl: './approve-dialog.html',
  styleUrl: './approve-dialog.css',
})
export class ApproveDialog implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(HospitalApi);

  readonly item = input.required<VerificationListItem>();

  readonly approved = output<void>();
  readonly cancelled = output<void>();
  // The record changed on the server (already decided, or gone). Carries the reason to show.
  readonly stale = output<string>();

  protected readonly options = BLOOD_GROUP_OPTIONS;
  protected readonly labels = BLOOD_GROUP_LABELS;

  protected readonly form = this.fb.group({
    verifiedBloodGroup: ['' as BloodGroup | '', [Validators.required]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.form.controls.verifiedBloodGroup.setValue(this.item().declaredBloodGroup);
  }

  protected fieldError(): string | null {
    return errorMessage(this.form.controls.verifiedBloodGroup);
  }

  protected submit(): void {
    if (this.busy()) {
      return;
    }

    const group = this.form.controls.verifiedBloodGroup.value;
    if (this.form.invalid || group === '') {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.error.set(null);

    this.api.approveVerification(this.item().id, group).subscribe({
      next: () => {
        this.busy.set(false);
        this.approved.emit();
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