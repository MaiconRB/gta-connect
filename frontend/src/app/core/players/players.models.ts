import { GameTitle, Platform } from '../auth/auth.models';
import { AvailabilityTag, PlaystyleTag, Region } from '../profile/profile.models';

export interface PlayerSummary {
  id: string;
  displayName: string;
  platform: Platform;
  gameTitle: GameTitle;
  bio: string | null;
  playstyleTags: number;
  availabilityTags: number;
  region: Region | null;
  hoursPlayed: number;
  favoriteModes: string | null;
  avatarPath: string | null;
  createdAtUtc: string;
  averageRating: number | null;
  ratingCount: number;
  isOnline: boolean;
}

// undefined em qualquer campo = filtro não aplicado (query param omitido pelo service).
export interface SearchFilters {
  platform?: Platform;
  playstyleTags?: number;
  region?: Region;
  availabilityTags?: number;
  page?: number;
  pageSize?: number;
}

