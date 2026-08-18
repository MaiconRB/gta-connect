import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { REPORT_REASON_OPTIONS, ReportReason } from '../../../core/moderation/moderation.models';
import { ModerationService } from '../../../core/moderation/moderation.service';
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
  imports: [RouterLink, TranslatePipe, FormsModule],
  templateUrl: './player-detail.html',
  styleUrl: './player-detail.css',
})
export class PlayerDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly playersService = inject(PlayersService);
  private readonly moderationService = inject(ModerationService);

  protected readonly playstyleTagOptions = PLAYSTYLE_TAG_OPTIONS;
  protected readonly availabilityTagOptions = AVAILABILITY_TAG_OPTIONS;
  protected readonly regionOptions = REGION_OPTIONS;
  protected readonly reportReasonOptions = REPORT_REASON_OPTIONS;

  protected readonly player = signal<PlayerSummary | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly isBlocking = signal(false);
  protected readonly blockError = signal<string | null>(null);
  protected readonly justBlocked = signal(false);

  protected readonly showReportForm = signal(false);
  protected readonly reportReason = signal<ReportReason>(ReportReason.Toxicidade);
  protected readonly reportDetails = signal('');
  protected readonly isReporting = signal(false);
  protected readonly reportError = signal<string | null>(null);
  protected readonly reportSent = signal(false);

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

  protected block(): void {
    const playerId = this.player()?.id;
    if (!playerId) {
      return;
    }

    this.isBlocking.set(true);
    this.blockError.set(null);

    this.moderationService.block(playerId).subscribe({
      next: () => {
        this.isBlocking.set(false);
        this.justBlocked.set(true);
      },
      error: (error: HttpErrorResponse) => {
        this.blockError.set(extractErrorMessage(error));
        this.isBlocking.set(false);
      },
    });
  }

  protected toggleReportForm(): void {
    this.showReportForm.update((value) => !value);
    this.reportSent.set(false);
    this.reportError.set(null);
  }

  protected submitReport(): void {
    const playerId = this.player()?.id;
    if (!playerId) {
      return;
    }

    this.isReporting.set(true);
    this.reportError.set(null);

    const details = this.reportDetails().trim();

    this.moderationService.report(playerId, this.reportReason(), details === '' ? null : details).subscribe({
      next: () => {
        this.isReporting.set(false);
        this.reportSent.set(true);
        this.showReportForm.set(false);
        this.reportDetails.set('');
      },
      error: (error: HttpErrorResponse) => {
        this.reportError.set(extractErrorMessage(error));
        this.isReporting.set(false);
      },
    });
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
