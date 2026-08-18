export interface Post {
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

export interface LikeToggleResult {
  liked: boolean;
  likeCount: number;
}
