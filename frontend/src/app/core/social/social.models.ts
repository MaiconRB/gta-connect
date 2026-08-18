export enum ConnectionStatus {
  Pending = 1,
  Accepted = 2,
  Declined = 3,
}

export interface ConnectionSummary {
  connectionId: string;
  otherProfileId: string;
  otherDisplayName: string;
  otherAvatarPath: string | null;
  status: ConnectionStatus;
  isRequester: boolean;
  createdAtUtc: string;
}

export interface ConnectionStatusInfo {
  connectionId: string | null;
  status: ConnectionStatus | null;
  isRequester: boolean | null;
  myRatingScore: number | null;
}
