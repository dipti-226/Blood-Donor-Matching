import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LoginResponse } from '../models/auth.models';
import { Roles } from '../models/enums';
import { getTokenExpiryMs } from './jwt';
import { roleLanding } from './role-landing';

const STORAGE_KEY = 'bd.session';
const MAX_TIMEOUT_MS = 2_147_483_647;

export type SignOutReason = 'manual' | 'expired' | 'unauthorized';

export interface SessionUser {
  userId: string;
  email: string;
  roles: string[];
}

interface StoredSession extends SessionUser {
  accessToken: string;
}

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly router = inject(Router);
  private readonly state = signal<StoredSession | null>(null);
  private expiryTimer: ReturnType<typeof setTimeout> | null = null;

  readonly isAuthenticated = computed(() => this.state() !== null);
  readonly token = computed(() => this.state()?.accessToken ?? null);
  readonly user = computed<SessionUser | null>(() => {
    const session = this.state();
    return session ? { userId: session.userId, email: session.email, roles: session.roles } : null;
  });

  constructor() {
    this.restore();
  }

  // Starts a session from a login response. Returns false if the token has no usable expiry.
  start(response: LoginResponse): boolean {
    const expiry = getTokenExpiryMs(response.accessToken);
    if (expiry === null || expiry <= Date.now()) {
      return false;
    }

    const session: StoredSession = {
      userId: response.userId,
      email: response.email,
      roles: response.roles,
      accessToken: response.accessToken,
    };

    this.state.set(session);
    this.save(session);
    this.scheduleExpiry(expiry);
    return true;
  }

  signOut(reason: SignOutReason = 'manual', navigate = true): void {
    this.clearTimer();
    this.state.set(null);
    this.clearStorage();

    if (navigate) {
      void this.router.navigate(['/login'], {
        queryParams: reason === 'manual' ? {} : { reason },
      });
    }
  }

  hasRole(role: string): boolean {
    return this.state()?.roles.includes(role) ?? false;
  }

  hasAnyRole(roles: readonly string[]): boolean {
    return roles.some((role) => this.hasRole(role));
  }

  isHospitalUser(): boolean {
    return this.hasRole(Roles.HospitalAdmin) || this.hasRole(Roles.HospitalStaff);
  }

  homeRoute(): string | null {
    const session = this.state();
    return session ? roleLanding(session.roles) : null;
  }

  private restore(): void {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return;
      }

      const parsed = JSON.parse(raw) as Partial<StoredSession>;
      if (
        typeof parsed.accessToken !== 'string' ||
        typeof parsed.userId !== 'string' ||
        typeof parsed.email !== 'string' ||
        !Array.isArray(parsed.roles)
      ) {
        this.clearStorage();
        return;
      }

      const expiry = getTokenExpiryMs(parsed.accessToken);
      if (expiry === null || expiry <= Date.now()) {
        this.clearStorage();
        return;
      }

      this.state.set({
        userId: parsed.userId,
        email: parsed.email,
        roles: parsed.roles as string[],
        accessToken: parsed.accessToken,
      });
      this.scheduleExpiry(expiry);
    } catch {
      this.clearStorage();
    }
  }

  private scheduleExpiry(expiryMs: number): void {
    this.clearTimer();
    const delay = Math.max(expiryMs - Date.now(), 0);
    this.expiryTimer = setTimeout(() => this.signOut('expired'), Math.min(delay, MAX_TIMEOUT_MS));
  }

  private clearTimer(): void {
    if (this.expiryTimer !== null) {
      clearTimeout(this.expiryTimer);
      this.expiryTimer = null;
    }
  }

  private save(session: StoredSession): void {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } catch {
      // Storage unavailable: the session still works until the page is reloaded.
    }
  }

  private clearStorage(): void {
    try {
      sessionStorage.removeItem(STORAGE_KEY);
    } catch {
      // Nothing to clear.
    }
  }
}