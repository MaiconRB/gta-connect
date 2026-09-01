import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { REPORT_REASON_OPTIONS, ReportReason } from '../../../core/moderation/moderation.models';
import { ModerationService } from '../../../core/moderation/moderation.service';
import {
  activeOptionLabels,
  activePlaystyleTags,
  AVAILABILITY_TAG_OPTIONS,
  PLAYSTYLE_TAG_OPTIONS,
  Region,
  REGION_OPTIONS,
} from '../../../core/profile/profile.models';
import { PlayerSummary } from '../../../core/players/players.models';
import { PlayersService } from '../../../core/players/players.service';
import { ConnectionStatus, ConnectionStatusInfo } from '../../../core/social/social.models';
import { SocialService } from '../../../core/social/social.service';
import { IconComponent } from '../../../shared/icon/icon';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';
import { LoadingTextComponent } from '../../../shared/loading-text/loading-text';
import { ToastService } from '../../../core/toast/toast.service';
import { ConfirmDialogService } from '../../../core/confirm/confirm-dialog.service';

@Component({
  selector: 'app-player-detail',
  imports: [RouterLink, TranslatePipe, FormsModule, IconComponent, PlayerAvatarComponent, ErrorMessageComponent, LoadingTextComponent],
  templateUrl: './player-detail.html',
  styleUrl: './player-detail.css',
})
export class PlayerDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly playersService = inject(PlayersService);
  private readonly moderationService = inject(ModerationService);
  private readonly socialService = inject(SocialService);
  private readonly toast = inject(ToastService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly translate = inject(TranslateService);

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
  protected readonly justBlocked = signal(false);

  protected readonly showReportForm = signal(false);
  protected readonly reportReason = signal<ReportReason>(ReportReason.Toxicidade);
  protected readonly reportDetails = signal('');
  protected readonly isReporting = signal(false);
  protected readonly reportSent = signal(false);

  protected readonly connectionStatus = signal<ConnectionStatusInfo | null>(null);
  protected readonly isConnectionActionPending = signal(false);

  protected readonly isLoggingSession = signal(false);

  protected readonly ratingScore = signal(0);
  protected readonly ratingComment = signal('');
  protected readonly completedSession = signal(true);
  protected readonly knewWhatToDo = signal(true);
  protected readonly wasToxic = signal(false);
  protected readonly isRating = signal(false);
  protected readonly ratingSent = signal(false);

  private playerId = '';

  constructor() {
    // Sempre presente — a rota /jogadores/:id exige o param (ver app.routes.ts).
    this.playerId = this.route.snapshot.paramMap.get('id')!;
    this.loadPlayer(this.playerId);
    this.loadConnectionStatus();
  }

  protected activeTagLabels(playstyleTags: number) {
    return activePlaystyleTags(playstyleTags);
  }

  protected activeAvailabilityLabels(availabilityTags: number): string[] {
    return activeOptionLabels(availabilityTags, this.availabilityTagOptions);
  }

  protected regionLabel(region: Region | null): string | null {
    return this.regionOptions.find((option) => option.value === region)?.labelKey ?? null;
  }

  protected async block(): Promise<void> {
    const playerId = this.player()?.id;
    if (!playerId) {
      return;
    }

    const confirmed = await this.confirmDialog.confirm({
      title: this.translate.instant('moderation.blockConfirmTitle'),
      message: this.translate.instant('moderation.blockConfirmMessage'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.isBlocking.set(true);

    this.moderationService.block(playerId).subscribe({
      next: () => {
        this.isBlocking.set(false);
        this.justBlocked.set(true);
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.isBlocking.set(false);
      },
    });
  }

  protected toggleReportForm(): void {
    this.showReportForm.update((value) => !value);
    this.reportSent.set(false);
  }

  protected submitReport(): void {
    const playerId = this.player()?.id;
    if (!playerId) {
      return;
    }

    this.isReporting.set(true);

    const details = this.reportDetails().trim();

    this.moderationService.report(playerId, this.reportReason(), details === '' ? null : details).subscribe({
      next: () => {
        this.isReporting.set(false);
        this.reportSent.set(true);
        this.showReportForm.set(false);
        this.reportDetails.set('');
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.isReporting.set(false);
      },
    });
  }

  protected connect(): void {
    this.isConnectionActionPending.set(true);

    this.socialService.sendRequest(this.playerId).subscribe({
      next: () => {
        this.isConnectionActionPending.set(false);
        this.loadConnectionStatus();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
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

    this.socialService.accept(connectionId).subscribe({
      next: () => {
        this.isConnectionActionPending.set(false);
        this.loadConnectionStatus();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
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

    this.socialService.decline(connectionId).subscribe({
      next: () => {
        this.isConnectionActionPending.set(false);
        this.loadConnectionStatus();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.isConnectionActionPending.set(false);
      },
    });
  }

  protected setRatingScore(score: number): void {
    this.ratingScore.set(score);
  }

  protected logSession(): void {
    const connectionId = this.connectionStatus()?.connectionId;
    if (!connectionId) {
      return;
    }

    this.isLoggingSession.set(true);

    this.socialService.logSession(connectionId).subscribe({
      next: () => {
        this.isLoggingSession.set(false);
        this.loadConnectionStatus();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.isLoggingSession.set(false);
      },
    });
  }

  protected submitRating(): void {
    const gameSessionId = this.connectionStatus()?.latestSessionId;
    if (!gameSessionId || this.ratingScore() < 1) {
      return;
    }

    this.isRating.set(true);
    this.ratingSent.set(false);

    const comment = this.ratingComment().trim();

    this.socialService
      .rate(gameSessionId, this.ratingScore(), comment === '' ? null : comment, this.completedSession(), this.knewWhatToDo(), this.wasToxic())
      .subscribe({
        next: () => {
          this.isRating.set(false);
          this.ratingSent.set(true);
          this.loadConnectionStatus();
          this.loadPlayer(this.playerId);
        },
        error: (error: HttpErrorResponse) => {
          this.toast.error(extractErrorMessage(error));
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
