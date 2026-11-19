import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { SessionService } from '../auth/session.service';
import { ApiProblem } from '../models/api-problem.model';
import { ToastService } from '../ui/toast.service';
import { ApiError } from './api-error';

function parseRetryAfter(value: string | null): number | null {
  const seconds = value ? Number.parseInt(value, 10) : Number.NaN;
  return Number.isFinite(seconds) && seconds > 0 ? seconds : null;
}

function messageFor(
  status: number,
  problem: ApiProblem | null,
  retryAfter: number | null,
  isLoginCall: boolean,
): string {
  switch (status) {
    case 0:
      return 'Cannot reach the server. Check your connection and try again.';
    case 400:
      if (problem?.errors && Object.keys(problem.errors).length > 0) {
        return 'Please check the highlighted fields.';
      }
      return problem?.detail ?? 'The request was not accepted.';
    case 401:
      return isLoginCall ? 'Email or password is incorrect.' : 'Your session has ended. Please sign in again.';
    case 403:
      return 'You are not allowed to do that.';
    case 404:
      return 'We could not find what you were looking for.';
    case 409:
      return problem?.detail ?? 'This action is no longer possible.';
    case 413:
      return 'That is too large to send.';
    case 429:
      return retryAfter
        ? `Too many attempts. Try again in ${retryAfter} seconds.`
        : 'Too many attempts. Please try again shortly.';
    default:
      return 'Something went wrong. Please try again.';
  }
}

// Turns every failed call into an ApiError and handles the cases that are the same everywhere.
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const session = inject(SessionService);
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const problem =
        typeof error.error === 'object' && error.error !== null ? (error.error as ApiProblem) : null;
      const retryAfter = parseRetryAfter(error.headers.get('Retry-After'));
      const isLoginCall = req.url.endsWith('/api/auth/login');

      const apiError = new ApiError(
        error.status,
        messageFor(error.status, problem, retryAfter, isLoginCall),
        problem?.errors ?? {},
        retryAfter,
      );

      if (error.status === 401 && !isLoginCall) {
        session.signOut('unauthorized');
      } else if (error.status === 403) {
        toast.error(apiError.message);
      }

      return throwError(() => apiError);
    }),
  );
};