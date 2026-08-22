// Espelha os enums do backend (GtaConnect.Domain.Enums). Os valores numéricos
// precisam bater exatamente com os enums em C# — é um contrato implícito entre
// frontend e backend que vale a pena manter comentado aqui.
export enum Platform {
  Ps4 = 1,
  Ps5 = 2,
}

export enum GameTitle {
  GtaV = 1,
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  platform: Platform;
  gameTitle: GameTitle;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  email: string;
  displayName: string;
  emailConfirmed: boolean;
  isModerator: boolean;
}
