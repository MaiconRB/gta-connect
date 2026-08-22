import { ReportReason } from '../moderation/moderation.models';

export { ReportReason };

export enum ReportStatus {
  Pending = 1,
  Reviewed = 2,
}

export interface ReportedProfileSummary {
  profileId: string;
  displayName: string;
  avatarPath: string | null;
  pendingCount: number;
  totalCount: number;
  isBanned: boolean;
}

export interface ReportDetail {
  id: string;
  reporterDisplayName: string;
  reason: ReportReason;
  details: string | null;
  status: ReportStatus;
  createdAtUtc: string;
  reviewedAtUtc: string | null;
}

export interface ReportedPost {
  id: string;
  authorProfileId: string;
  authorDisplayName: string;
  authorAvatarPath: string | null;
  content: string | null;
  photoPath: string | null;
  createdAtUtc: string;
  likeCount: number;
  likedByMe: boolean;
}

export interface ReportedProfileDetail {
  profileId: string;
  displayName: string;
  avatarPath: string | null;
  isBanned: boolean;
  reports: ReportDetail[];
  recentPosts: ReportedPost[];
}
