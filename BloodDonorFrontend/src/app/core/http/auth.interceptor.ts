import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { SessionService } from '../auth/session.service';

// Adds the bearer token to calls to our own API only.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(SessionService).token();

  if (token && req.url.startsWith(environment.apiBaseUrl)) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req);
};