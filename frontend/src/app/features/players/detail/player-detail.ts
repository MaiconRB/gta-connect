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
import { ConnectionStatus, ConnectionStatusInfo } from '../../../core/social/social.models';
import { SocialService } from '../../../core/social/social.service';

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
  private readonly socialService = inject(SocialService);

  protected readonly ConnectionStatus = ConnectionStatus;
  protected readonly ratingStars = [1, 2, 3, 4, 5];

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

  protected readonly connectionStatus = signal<ConnectionStatusInfo | null>(null);
  protected readonly isConnectionActionPending = signal(false);
  protected readonly connectionError = signal<string | null>(null);

  protected readonly ratingScore = signal(0);
  protected readonly ratingComment = signal('');
  protected readonly isRating = signal(false);
  protected readonly ratingError = signal<string | null>(null);
  protected readonly ratingSent = signal(false);

  private playerId = '';

  constructor() {
    // Sempre presente — a rota /jogadores/:id exige o param (ver app.routes.ts).
    this.playerId = this.route.snapshot.paramMap.get('id')!;
    this.loadPlayer(this.playerId);
    this.loadConnectionStatus();
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

  protected connect(): void {
    this.isConnectionActionPending.set(true);
    this.connectionError.set(null);

    this.socialService.sendRequest(this.playerId).subscribe({
      next: () => {
        this.isConnectionActionPending.set(false);
        this.socialService.notifyChanged();
        this.loadConnectionStatus();
      },
      error: (error: HttpErrorResponse) => {
        this.connectionError.set(extractErrorMessage(error));
        this.isConnectionActionPending.set(false);
      },
    });
  }

  protected acceptConnection(): void {
    const connectionId = this.connectionStatus()?.connectionId;
    if (!connectionId) {
      return;
    }

    this.isConnectionActionPending.set(true);
    this.connectionError.set(null);

    this.socialService.accept(connectionId).subscribe({
      next: () => {
        this.isConnectionActionPending.set(false);
        this.socialService.notifyChanged();
        this.loadConnectionStatus();
      },
      error: (error: HttpErrorResponse) => {
        this.connectionError.set(extractErrorMessage(error));
        this.isConnectionActionPending.set(false);
      },
    });
  }

  protected declineConnection(): void {
    const connectionId = this.connectionStatus()?.connectionId;
    if (!connectionId) {
      return;
    }

    this.isConnectionActionPending.set(true);
    this.connectionError.set(null);

    this.socialService.decline(connectionId).subscribe({
      next: () => {
        this.isConnectionActionPending.set(false);
        this.socialService.notifyChanged();
        this.loadConnectionStatus();
      },
      error: (error: HttpErrorResponse) => {
        this.connectionError.set(extractErrorMessage(error));
        this.isConnectionActionPending.set(false);
      },
    });
  }

  protected setRatingScore(score: number): void {
    this.ratingScore.set(score);
  }

  protected submitRating(): void {
    if (this.ratingScore() < 1) {
      return;
    }

    this.isRating.set(true);
    this.ratingError.set(null);
    this.ratingSent.set(false);

    const comment = this.ratingComment().trim();

    this.socialService.rate(this.playerId, this.ratingScore(), comment === '' ? null : comment).subscribe({
      next: () => {
        this.isRating.set(false);
        this.ratingSent.set(true);
        this.loadConnectionStatus();
        this.loadPlayer(this.playerId);
      },
      error: (error: HttpErrorResponse) => {
        this.ratingError.set(extractErrorMessage(error));
        this.isRating.set(false);
      },
    });
  }

  private loadConnectionStatus(): void {
    this.socialService.getConnectionStatus(this.playerId).subscribe({
      next: (status) => {
        this.connectionStatus.set(status);
        if (status.myRatingScore) {
          this.ratingScore.set(status.myRatingScore);
        }
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
