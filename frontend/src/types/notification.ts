export type NotificationType =
  | 'ProjectSubmittedForReview'
  | 'ProjectApproved'
  | 'ProjectRejected'
  | string;

export interface InAppNotification {
  id: number;
  type: NotificationType;
  title: string;
  message: string;
  projectId?: number;
  targetUrl?: string;
  isRead: boolean;
  createdAtUtc: string;
  readAtUtc?: string;
}

export interface UnreadCountResponse {
  unreadCount: number;
}
