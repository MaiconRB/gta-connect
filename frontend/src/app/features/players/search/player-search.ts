import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Platform } from '../../../core/auth/auth.models';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import {
  activeOptionLabels,
  activePlaystyleTags,
  AVAILABILITY_TAG_OPTIONS,
  AvailabilityTag,
  PLAYSTYLE_TAG_OPTIONS,
  PlaystyleTag,
  Region,
  REGION_OPTIONS,
} from '../../../core/profile/profile.models';
import { PlayerSummary } from '../../../core/players/players.models';
import { PlayersService } from '../../../core/players/players.service';
import { ProfileService } from '../../../core/profile/profile.service';
import { IconComponent, IconName } from '../../../shared/icon/icon';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';

const PAGE_SIZE = 20;
const MAX_VISIBLE_BADGES = 3;

interface DisplayBadge {
  labelKey: string;
  variant: 'playstyle' | 'availability';
  iconName?: IconName;
}

@Component({
  selector: 'app-player-search',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, IconComponent, PlayerAvatarComponent, ErrorMessageComponent],
  templateUrl: './player-search.html',
  styleUrl: './player-search.css',
})
export class PlayerSearch {
  private readonly playersService = inject(PlayersService);
  private readonly profileService = inject(ProfileService);
  private readonly formBuilder = inject(NonNullableFormBuilder);

  // Minhas próprias tags/região — só pra destacar "por que esse match" nos cards (quantas
  // tags em comum). Não influencia a ordenação: o score de compatibilidade já é calculado
  // no banco (ver PlayerProfileRepository.SearchAsync), isso aqui é só apresentação.
  private myPlaystyleTags = 0;
  private myAvailabilityTags = 0;
  private myRegion: Region | null = null;

  protected readonly playstyleTagOptions = PLAYSTYLE_TAG_OPTIONS;
  protected readonly availabilityTagOptions = AVAILABILITY_TAG_OPTIONS;
  protected readonly regionOptions = REGION_OPTIONS;

  protected readonly selectedTags = signal<Set<PlaystyleTag>>(new Set());
  protected readonly selectedAvailabilityTags = signal<Set<AvailabilityTag>>(new Set());

  // platform/region ficam como string no form ('' = sem filtro) — mesmo tratamento
  // já usado em profile.ts pra evitar FormControl<T | null> tipado.
  protected readonly form = this.formBuilder.group({
    platform: [''],
    region: [''],
  });

  protected readonly results = signal<PlayerSummary[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly page = signal(1);
  protected readonly hasSearched = signal(false);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.profileService.getMyProfile().subscribe({
      next: (profile) => {
        this.myPlaystyleTags = profile.playstyleTags;
        this.myAvailabilityTags = profile.availabilityTags;
        this.myRegion = profile.region;
      },
    });

    // Abre a tela já com sugestões (sem exigir clique em "Buscar") — a busca sem filtro
    // nenhum já vem ordenada por compatibilidade pelo backend.
    this.runSearch();
  }

  // Quantas tags (estilo + disponibilidade) esse jogador compartilha comigo — a parte
  // visível do "por que esse match" que o backend não devolve como número pronto.
  protected sharedInterestCount(player: PlayerSummary): number {
    return this.countSharedBits(player.playstyleTags, this.myPlaystyleTags) + this.countSharedBits(player.availabilityTags, this.myAvailabilityTags);
  }

  protected sharesMyRegion(player: PlayerSummary): boolean {
    return this.myRegion !== null && player.region === this.myRegion;
  }

  private countSharedBits(a: number, b: number): number {
    let shared = a & b;
    let count = 0;
    while (shared > 0) {
      count += shared & 1;
      shared >>= 1;
    }
    return count;
  }

  protected activeTagLabels(playstyleTags: number) {
    return activePlaystyleTags(playstyleTags);
  }

  protected activeAvailabilityLabels(availabilityTags: number): string[] {
    return activeOptionLabels(availabilityTags, this.availabilityTagOptions);
  }

  // Card de busca é compacto — mostra no máximo 3 badges (misturando estilo de jogo
  // e disponibilidade) e resume o resto em "+N". Lista completa continua só no detalhe.
  protected visibleBadges(player: PlayerSummary): DisplayBadge[] {
    return this.combinedBadges(player).slice(0, MAX_VISIBLE_BADGES);
  }

  protected hiddenBadgeCount(player: PlayerSummary): number {
    return Math.max(0, this.combinedBadges(player).length - MAX_VISIBLE_BADGES);
  }

  private combinedBadges(player: PlayerSummary): DisplayBadge[] {
    return [
      ...this.activeTagLabels(player.playstyleTags).map((tag) => ({ labelKey: tag.labelKey, variant: 'playstyle' as const, iconName: tag.iconName })),
      ...this.activeAvailabilityLabels(player.availabilityTags).map((labelKey) => ({ labelKey, variant: 'availability' as const })),
    ];
  }

  protected regionLabel(region: Region | null): string | null {
    return this.regionOptions.find((option) => option.value === region)?.labelKey ?? null;
  }

  protected toggleTag(tag: PlaystyleTag): void {
    const updated = new Set(this.selectedTags());
    if (updated.has(tag)) {
      updated.delete(tag);
    } else {
      updated.add(tag);
    }
    this.selectedTags.set(updated);
  }

  protected isTagSelected(tag: PlaystyleTag): boolean {
    return this.selectedTags().has(tag);
  }

  protected toggleAvailabilityTag(tag: AvailabilityTag): void {
    const updated = new Set(this.selectedAvailabilityTags());
    if (updated.has(tag)) {
      updated.delete(tag);
    } else {
      updated.add(tag);
    }
    this.selectedAvailabilityTags.set(updated);
  }

  protected isAvailabilityTagSelected(tag: AvailabilityTag): boolean {
    return this.selectedAvailabilityTags().has(tag);
  }

  protected search(): void {
    this.page.set(1);
    this.runSearch();
  }

  protected goToPage(page: number): void {
    this.page.set(page);
    this.runSearch();
  }

  protected get hasNextPage(): boolean {
    return this.page() * PAGE_SIZE < this.totalCount();
  }

  protected get hasPreviousPage(): boolean {
    return this.page() > 1;
  }

  protected get hasActiveFilters(): boolean {
    const { platform, region } = this.form.getRawValue();
    return platform !== '' || region !== '' || this.selectedTags().size > 0 || this.selectedAvailabilityTags().size > 0;
  }

  private runSearch(): void {
    const { platform, region } = this.form.getRawValue();
    const playstyleTags = Array.from(this.selectedTags()).reduce((mask, tag) => mask | tag, 0);
    const availabilityTags = Array.from(this.selectedAvailabilityTags()).reduce((mask, tag) => mask | tag, 0);

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.hasSearched.set(true);

    this.playersService
      .searchPlayers({
        platform: platform === '' ? undefined : (Number(platform) as Platform),
        region: region === '' ? undefined : (Number(region) as Region),
        playstyleTags: playstyleTags === 0 ? undefined : playstyleTags,
        availabilityTags: availabilityTags === 0 ? undefined : availabilityTags,
        page: this.page(),
        pageSize: PAGE_SIZE,
      })
      .subscribe({
        next: (result) => {
          this.results.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          this.isLoading.set(false);
        },
      });
  }
}
