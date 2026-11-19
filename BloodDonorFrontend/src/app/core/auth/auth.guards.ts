import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Role } from '../models/enums';
import { SessionService } from './session.service';

// Guards are a convenience for the user. The API enforces the same rules again.

export const authGuard: CanActivateFn = (_route, state) => {
  const session = inject(SessionService);
  const router = inject(Router);

  return session.isAuthenticated()
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

export const guestGuard: CanActivateFn = () => {
  const session = inject(SessionService);
  const router = inject(Router);

  return session.isAuthenticated() ? router.createUrlTree([session.homeRoute() ?? '/forbidden']) : true;
};

export const roleGuard: CanActivateFn = (route) => {
  const session = inject(SessionService);
  const router = inject(Router);
  const allowed = route.data['roles'] as readonly Role[] | undefined;

  return !allowed || session.hasAnyRole(allowed) ? true : router.createUrlTree(['/forbidden']);
};