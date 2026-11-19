import { Component, DestroyRef, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthApi } from '../../../core/auth/auth-api.service';
import { HospitalContextService } from '../../../core/auth/hospital-context.service';
import { SessionService } from '../../../core/auth/session.service';
import { ApiError } from '../../../core/http/api-error';
import { LoginResponse } from '../../../core/models/auth.models';
import { FormField } from '../../../shared/ui/form-field/form-field';
import { errorMessage } from '../../../shared/utils/form-errors';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, FormField],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(AuthApi);
  private readonly session = inject(SessionService);
  private readonly hospitalContext = inject(HospitalContextService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private countdownTimer: ReturnType<typeof setInterval> | null = null;

  protected readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly retrySeconds = signal(0);
  protected readonly notice: string | null;

  constructor() {
    const reason = this.route.snapshot.queryParamMap.get('reason');
    this.notice =
      reason === 'expired' || reason === 'unauthorized' ? 'Your session has ended. Please sign in again.' : null;

    inject(DestroyRef).onDestroy(() => this.stopCountdown());
  }

  protected fieldError(name: 'email' | 'password'): string | null {
    return errorMessage(this.form.controls[name]);
  }

  protected submit(): void {
    if (this.busy() || this.retrySeconds() > 0) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.error.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: (response) => this.onLoggedIn(response),
      error: (err: unknown) => this.onLoginError(err),
    });
  }

  private onLoggedIn(response: LoginResponse): void {
    if (!this.session.start(response)) {
      this.fail('We could not start your session. Please try again.');
      return;
    }

    const home = this.session.homeRoute();
    if (!home) {
      this.session.signOut('manual', false);
      this.fail('This account type is not available in the app yet.');
      return;
    }

    if (this.session.isHospitalUser()) {
      this.hospitalContext.load().subscribe({
        next: () => this.finish(home),
        error: () => {
          this.session.signOut('manual', false);
          this.fail('We could not load your hospital. Please try again or contact your administrator.');
        },
      });
      return;
    }

    this.finish(home);
  }

  private finish(home: string): void {
    this.busy.set(false);
    void this.router.navigateByUrl(this.target(home));
  }

  // Only same-site paths are accepted as a return address.
  private target(home: string): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//') ? returnUrl : home;
  }

  private onLoginError(err: unknown): void {
    if (err instanceof ApiError) {
      if (err.status === 429 && err.retryAfterSeconds) {
        this.startCountdown(err.retryAfterSeconds);
      }
      this.fail(err.message);
      return;
    }

    this.fail('Something went wrong. Please try again.');
  }

  private fail(message: string): void {
    this.busy.set(false);
    this.error.set(message);
  }

  private startCountdown(seconds: number): void {
    this.stopCountdown();
    this.retrySeconds.set(seconds);

    this.countdownTimer = setInterval(() => {
      const remaining = this.retrySeconds() - 1;
      this.retrySeconds.set(Math.max(remaining, 0));

      if (remaining <= 0) {
        this.stopCountdown();
      }
    }, 1000);
  }

  private stopCountdown(): void {
    if (this.countdownTimer !== null) {
      clearInterval(this.countdownTimer);
      this.countdownTimer = null;
    }
  }
}