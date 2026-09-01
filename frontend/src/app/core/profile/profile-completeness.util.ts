import { ProfileResponse } from './profile.models';

// Pesos alinhados ao score de compatibilidade real (PlayerProfileRepository.SearchAsync,
// Fase 1): PlaystyleTags/AvailabilityTags/Region são o que pesa pra valer no matching, por
// isso dominam a completude também — "todos os campos pesam igual" seria enganoso, um perfil
// só com bio preenchida "parece" mais completo do que na prática contribui pro próprio match.
const WEIGHTS = {
  playstyleTags: 30,
  availabilityTags: 25,
  region: 20,
  bio: 10,
  avatarPath: 10,
  extras: 5, // favoriteModes OU hoursPlayed > 0 — qualquer um dos dois já conta
} as const;

export interface MissingField {
  labelKey: string;
  weight: number;
}

function isFieldComplete(profile: ProfileResponse, field: keyof typeof WEIGHTS): boolean {
  switch (field) {
    case 'playstyleTags':
      return profile.playstyleTags !== 0;
    case 'availabilityTags':
      return profile.availabilityTags !== 0;
    case 'region':
      return profile.region !== null;
    case 'bio':
      return profile.bio !== null && profile.bio.trim() !== '';
    case 'avatarPath':
      return profile.avatarPath !== null;
    case 'extras':
      return (profile.favoriteModes !== null && profile.favoriteModes.trim() !== '') || profile.hoursPlayed > 0;
  }
}

export function calculateCompleteness(profile: ProfileResponse): number {
  return (Object.keys(WEIGHTS) as (keyof typeof WEIGHTS)[])
    .filter((field) => isFieldComplete(profile, field))
    .reduce((total, field) => total + WEIGHTS[field], 0);
}

const MISSING_FIELD_LABEL_KEYS: Record<keyof typeof WEIGHTS, string> = {
  playstyleTags: 'profile.playstyleLabel',
  availabilityTags: 'profile.availabilityLabel',
  region: 'profile.regionLabel',
  bio: 'profile.bioLabel',
  avatarPath: 'profile.avatarChoose',
  extras: 'profile.favoriteModesLabel',
};

export function listMissingFields(profile: ProfileResponse): MissingField[] {
  return (Object.keys(WEIGHTS) as (keyof typeof WEIGHTS)[])
    .filter((field) => !isFieldComplete(profile, field))
    .map((field) => ({ labelKey: MISSING_FIELD_LABEL_KEYS[field], weight: WEIGHTS[field] }));
}
