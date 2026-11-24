import { Component, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HospitalApi } from '../../../../core/hospital/hospital-api.service';
import { ApiError } from '../../../../core/http/api-error';
import { FormField } from '../../../../shared/ui/form-field/form-field';
import { ModalDialog } from '../../../../shared/ui/modal-dialog/modal-dialog';
import { applyServerErrors, errorMessage } from '../../../../shared/utils/form-errors';
import { matchesPassword, passwordPolicy } from '../../../../shared/utils/validators';

type StaffField = 'email' | 'password' | 'confirmPassword';

@Component({
  selector: 'app-add-staff-dialog',
  imports: [ReactiveFormsModule, ModalDialog, FormField],
  templateUrl: './add-staff-dialog.html',
  styleUrl: './add-staff-dialog.css',
})
export class AddStaffDialog {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly api = inject(HospitalApi);

  readonly hospitalId = input.required<string>();

  readonly created = output<void>();
  readonly dismissed = output<void>();

  protected readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, passwordPolicy]],
    confirmPassword: ['', [Validators.required, matchesPassword]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    // The confirm field must be re-checked whenever the password changes.
    this.form.controls.password.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.confirmPassword.updateValueAndValidity());
  }

  protected fieldError(name: StaffField): string | null {
    return errorMessage(this.form.controls[name]);
  }

  protected submit(): void {
    if (this.busy()) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password, confirmPassword } = this.form.getRawValue();

    this.busy.set(true);
    this.error.set(null);

    this.api.createStaff(this.hospitalId(), { email: email.trim(), password, confirmPassword }).subscribe({
      next: () => {
        this.busy.set(false);
        this.created.emit();
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

    // An email that is already registered belongs under the email field.
    if (err.status === 409) {
      this.form.controls.email.setErrors({ server: err.message });
      this.form.controls.email.markAsTouched();
      return;
    }

    const unmatched = applyServerErrors(this.form, err);
    this.error.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
} 