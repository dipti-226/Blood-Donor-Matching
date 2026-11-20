import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SessionService } from '../../../core/auth/session.service';
import { Roles } from '../../../core/models/enums';
import { AppNotification } from '../../../core/models/notification.models';
import { NotificationsApi } from '../../../core/notifications/notifications-api.service';
import { UnreadCountService } from '../../../core/notifications/unread-count.service';
import { ToastService } from '../../../core/ui/toast.service';
import { RelativeTimePipe } from '../../../shared/pipes/relative-time';
import { EmptyState } from '../../../shared/ui/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/ui/error-banner/error-banner';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHeader } from '../../../shared/ui/page-header/page-header';
import { LOADING, LoadState, loadError } from '../../../shared/utils/load-state';

const ICONS: Record<string, string> = {
  DonorRequestReceived: 'bi-envelope',
  DonorRequestAccepted: 'bi-check-circle',
  DonorRequestDeclined: 'bi-x-circle',
  DonorRequestCancelled: 'bi-slash-circle',
  BloodRequestClosed: 'bi-flag',
};

@Component({
  selector: 'app-notifications',
  imports: [RouterLink, PageHeader, LoadingState, EmptyState, ErrorBanner, RelativeTimePipe],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css',
})
export class Notifications implements OnInit {
  private readonly api = inject(NotificationsApi);
  private readonly unread = inject(UnreadCountService);
  private readonly session = inject(SessionService);
  private readonly toast = inject(ToastService);

  protected readonly state = signal<LoadState<AppNotification[]>>(LOADING);
  protected readonly unreadOnly = signal(false);
  protected readonly busy = signal(false);

  protected readonly items = computed(() => {
    const state = this.state();
    return state.status === 'ready' ? state.data : [];
  });
  protected readonly errorText = computed(() => {
    const state = this.state();
    return state.status === 'error' ? state.message : '';
  });
  protected readonly hasUnread = computed(() => this.items().some((item) => !item.isRead));

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.state.set(LOADING);

    this.api.list(this.unreadOnly()).subscribe({
      next: (data) => this.state.set({ status: 'ready', data }),
      error: (err: unknown) => this.state.set(loadError(err)),
    });
  }

  protected setFilter(unreadOnly: boolean): void {
    if (this.unreadOnly() === unreadOnly) {
      return;
    }

    this.unreadOnly.set(unreadOnly);
    this.load();
  }

  protected markRead(notification: AppNotification): void {
    this.api.markRead(notification.id).subscribe({
      next: (updated) => {
        this.replace(updated);
        this.unread.refresh();
      },
      error: (err: unknown) => this.toast.error(this.messageOf(err)),
    });
  }

  protected markAllRead(): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);

    this.api.markAllRead().subscribe({
      next: () => {
        this.busy.set(false);
        this.unread.refresh();
        this.load();
      },
      error: (err: unknown) => {
        this.busy.set(false);
        this.toast.error(this.messageOf(err));
      },
    });
  }

  protected iconFor(type: string): string {
    return ICONS[type] ?? 'bi-bell';
  }

  // Where "View" leads: donors to their requests, hospital users to the related blood request.
  protected viewLink(notification: AppNotification): string | null {
    if (this.session.hasRole(Roles.Donor)) {
      return '/donor/requests';
    }
    return notification.bloodRequestId ? `/hospital/requests/${notification.bloodRequestId}` : null;
  }

  private replace(updated: AppNotification): void {
    const state = this.state();
    if (state.status !== 'ready') {
      return;
    }

    const data = this.unreadOnly()
      ? state.data.filter((item) => item.id !== updated.id)
      : state.data.map((item) => (item.id === updated.id ? updated : item));

    this.state.set({ status: 'ready', data });
  }

  private messageOf(error: unknown): string {
    return error instanceof Error ? error.message : 'Something went wrong. Please try again.';
  }
}