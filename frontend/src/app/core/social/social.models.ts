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
  // Sessão mais recente registrada pra essa conexão — Rating é por sessão, então "minha
  // nota" só existe amarrada a uma sessão (ver GameSession no backend).
  latestSessionId: string | null;
  myRatingScore: number | null;
}
