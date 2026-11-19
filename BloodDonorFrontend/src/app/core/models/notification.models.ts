import { NotificationType } from './enums';

export interface AppNotification {
  id: string;
  type: NotificationType;
  message: string;
  bloodRequestId: string | null;
  donorRequestId: string | null;
  isRead: boolean;
  createdAt: string;
  readAt: string | null;
}

export interface UnreadCount {
  unreadCount: number;
}

export interface MarkAllReadResult {
  markedCount: number;
}