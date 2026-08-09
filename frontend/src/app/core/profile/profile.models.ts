import { GameTitle, Platform } from '../auth/auth.models';

// Espelha GtaConnect.Domain.Enums.PlaystyleTag ([Flags], bitmask). Os valores numéricos
// precisam bater exatamente com o enum em C#.
export enum PlaystyleTag {
  MundoAbertoCalmo = 1 << 0,
  GrindDeHeist = 1 << 1,
  Corrida = 1 << 2,
  RolePlay = 1 << 3,
  FreemodeSocial = 1 << 4,
  CampanhaHistoria = 1 << 5,
  PvpCompetitivo = 1 << 6,
  NegociosEconomia = 1 << 7,
}

export const PLAYSTYLE_TAG_OPTIONS: { value: PlaystyleTag; labelKey: string }[] = [
  { value: PlaystyleTag.MundoAbertoCalmo, labelKey: 'profile.playstyleTags.mundoAbertoCalmo' },
  { value: PlaystyleTag.GrindDeHeist, labelKey: 'profile.playstyleTags.grindDeHeist' },
  { value: PlaystyleTag.Corrida, labelKey: 'profile.playstyleTags.corrida' },
  { value: PlaystyleTag.RolePlay, labelKey: 'profile.playstyleTags.rolePlay' },
  { value: PlaystyleTag.FreemodeSocial, labelKey: 'profile.playstyleTags.freemodeSocial' },
  { value: PlaystyleTag.CampanhaHistoria, labelKey: 'profile.playstyleTags.campanhaHistoria' },
  { value: PlaystyleTag.PvpCompetitivo, labelKey: 'profile.playstyleTags.pvpCompetitivo' },
  { value: PlaystyleTag.NegociosEconomia, labelKey: 'profile.playstyleTags.negociosEconomia' },
];

export interface ProfileResponse {
  id: string;
  displayName: string;
  platform: Platform;
  gameTitle: GameTitle;
  bio: string | null;
  playstyleTags: number;
  hoursPlayed: number;
  favoriteModes: string | null;
  avatarPath: string | null;
  createdAtUtc: string;
}

export interface UpdateProfileRequest {
  bio: string | null;
  playstyleTags: number;
  hoursPlayed: number;
  favoriteModes: string | null;
}
