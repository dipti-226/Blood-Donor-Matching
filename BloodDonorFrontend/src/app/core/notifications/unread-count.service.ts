import { Injectable, inject, signal } from '@angular/core';
import { SessionService } from '../auth/session.service';
import { NotificationsApi } from './notifications-api.service';

const POLL_INTERVAL_MS = 30_000;

// Keeps the bell badge current: one request every 30 seconds, only while signed in and the tab is visible.
@Injectable({ providedIn: 'root' })
export class UnreadCountService {
  private readonly api = inject(NotificationsApi);
  private readonly session = inject(SessionService);
  private timer: ReturnType<typeof setInterval> | null = null;

  readonly count = signal(0);

  private readonly onVisibilityChange = (): void => {
    if (!document.hidden) {
      this.refresh();
    }
  };

  start(): void {
    if (this.timer !== null) {
      return;
    }

    this.refresh();

    this.timer = setInterval(() => {
      if (!document.hidden && this.session.isAuthenticated()) {
        this.refresh();
      }
    }, POLL_INTERVAL_MS);

    document.addEventListener('visibilitychange', this.onVisibilityChange);
  }

  stop(): void {
    if (this.timer !== null) {
      clearInterval(this.timer);
      this.timer = null;
    }

    document.removeEventListener('visibilitychange', this.onVisibilityChange);
    this.count.set(0);
  }

  refresh(): void {
    if (!this.session.isAuthenticated()) {
      return;
    }

    this.api.unreadCount().subscribe({
      next: (result) => this.count.set(result.unreadCount),
      // A failed refresh is not worth interrupting the user. The next poll tries again.
      error: () => undefined,
    });
  }
}