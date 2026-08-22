export enum NotificationType {
  ConnectionRequestReceived = 1,
  MessageReceived = 2,
  RatingReceived = 3,
  PostLiked = 4,
}

export interface AppNotification {
  id: string;
  type: NotificationType;
  actorProfileId: string;
  actorDisplayName: string;
  actorAvatarPath: string | null;
  relatedEntityId: string | null;
  isRead: boolean;
  createdAtUtc: string;
}
