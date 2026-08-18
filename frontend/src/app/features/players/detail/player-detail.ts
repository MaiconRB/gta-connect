import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import {
  activeOptionLabels,
  AVAILABILITY_TAG_OPTIONS,
  PLAYSTYLE_TAG_OPTIONS,
  Region,
  REGION_OPTIONS,
} from '../../../core/profile/profile.models';
import { PlayerSummary } from '../../../core/players/players.models';
import { PlayersService } from '../../../core/players/players.service';

@Component({
  selector: 'app-player-detail',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './player-detail.html',
  styleUrl: './player-detail.css',
})
export class PlayerDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly playersService = inject(PlayersService);

  protected readonly playstyleTagOptions = PLAYSTYLE_TAG_OPTIONS;
  protected readonly availabilityTagOptions = AVAILABILITY_TAG_OPTIONS;
  protected readonly regionOptions = REGION_OPTIONS;

  protected readonly player = signal<PlayerSummary | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  constructor() {
    // Sempre presente — a rota /jogadores/:id exige o param (ver app.routes.ts).
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadPlayer(id);
  }

  protected activeTagLabels(playstyleTags: number): string[] {
    return activeOptionLabels(playstyleTags, this.playstyleTagOptions);
  }

  protected activeAvailabilityLabels(availabilityTags: number): string[] {
    return activeOptionLabels(availabilityTags, this.availabilityTagOptions);
  }

  protected regionLabel(region: Region | null): string | null {
    return this.regionOptions.find((option) => option.value === region)?.labelKey ?? null;
  }

  private loadPlayer(id: string): void {
    this.isLoading.set(true);
    this.playersService.getPlayerById(id).subscribe({
      next: (response) => {
        this.player.set(response);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
