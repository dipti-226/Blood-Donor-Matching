import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthApi } from '../../../core/auth/auth-api.service';
import { ApiError } from '../../../core/http/api-error';
import { Roles } from '../../../core/models/enums';
import { ToastService } from '../../../core/ui/toast.service';
import { FormField } from '../../../shared/ui/form-field/form-field';
import { applyServerErrors, errorMessage } from '../../../shared/utils/form-errors';
import { matchesPassword, passwordPolicy } from '../../../shared/utils/validators';

type RegisterField = 'email' | 'password' | 'confirmPassword';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, FormField],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(AuthApi);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

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

  protected fieldError(name: RegisterField): string | null {
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

    this.busy.set(true);
    this.error.set(null);

    const { email, password, confirmPassword } = this.form.getRawValue();

    this.auth.register({ email, password, confirmPassword, requestedRole: Roles.Donor }).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success('Account created. Please sign in.');
        void this.router.navigate(['/login']);
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

    const unmatched = applyServerErrors(this.form, err);
    this.error.set(unmatched.length > 0 ? unmatched.join(' ') : err.message);
  }
}