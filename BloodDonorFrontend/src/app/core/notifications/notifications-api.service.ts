import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClient } from '../http/api-client';
import { AppNotification, MarkAllReadResult, UnreadCount } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationsApi {
  private readonly api = inject(ApiClient);

  list(unreadOnly: boolean): Observable<AppNotification[]> {
    return this.api.get<AppNotification[]>('/api/notifications', { unreadOnly });
  }

  unreadCount(): Observable<UnreadCount> {
    return this.api.get<UnreadCount>('/api/notifications/unread-count');
  }

  markRead(id: string): Observable<AppNotification> {
    return this.api.post<AppNotification>(`/api/notifications/${id}/read`);
  }

  markAllRead(): Observable<MarkAllReadResult> {
    return this.api.post<MarkAllReadResult>('/api/notifications/read-all');
  }
}