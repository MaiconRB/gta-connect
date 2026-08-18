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

// Espelha GtaConnect.Domain.Enums.Region (seleção única, não [Flags]).
export enum Region {
  Norte = 1,
  Nordeste = 2,
  CentroOeste = 3,
  Sudeste = 4,
  Sul = 5,
}

export const REGION_OPTIONS: { value: Region; labelKey: string }[] = [
  { value: Region.Norte, labelKey: 'profile.region.norte' },
  { value: Region.Nordeste, labelKey: 'profile.region.nordeste' },
  { value: Region.CentroOeste, labelKey: 'profile.region.centroOeste' },
  { value: Region.Sudeste, labelKey: 'profile.region.sudeste' },
  { value: Region.Sul, labelKey: 'profile.region.sul' },
];

// Espelha GtaConnect.Domain.Enums.AvailabilityTag ([Flags], bitmask).
export enum AvailabilityTag {
  Manha = 1 << 0,
  Tarde = 1 << 1,
  Noite = 1 << 2,
  Madrugada = 1 << 3,
  FimDeSemana = 1 << 4,
}

export const AVAILABILITY_TAG_OPTIONS: { value: AvailabilityTag; labelKey: string }[] = [
  { value: AvailabilityTag.Manha, labelKey: 'profile.availabilityTags.manha' },
  { value: AvailabilityTag.Tarde, labelKey: 'profile.availabilityTags.tarde' },
  { value: AvailabilityTag.Noite, labelKey: 'profile.availabilityTags.noite' },
  { value: AvailabilityTag.Madrugada, labelKey: 'profile.availabilityTags.madrugada' },
  { value: AvailabilityTag.FimDeSemana, labelKey: 'profile.availabilityTags.fimDeSemana' },
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
  region: Region | null;
  availabilityTags: number;
  avatarPath: string | null;
  createdAtUtc: string;
}

export interface UpdateProfileRequest {
  bio: string | null;
  playstyleTags: number;
  hoursPlayed: number;
  favoriteModes: string | null;
  region: Region | null;
  availabilityTags: number;
}

// Pura e reutilizável — usada tanto no perfil (estilo de jogo, disponibilidade) quanto
// na busca de jogadores (mesmos grupos de tags em cards de resultado).
export function activeOptionLabels<T extends number>(bitmask: number, options: { value: T; labelKey: string }[]): string[] {
  return options.filter((option) => (bitmask & option.value) !== 0).map((option) => option.labelKey);
}

