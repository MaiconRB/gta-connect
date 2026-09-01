import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { REPORT_REASON_OPTIONS } from '../../../core/moderation/moderation.models';
import { ModerationReviewService } from '../../../core/moderation-review/moderation-review.service';
import { ReportedProfileDetail, ReportStatus } from '../../../core/moderation-review/moderation-review.models';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';
import { LoadingTextComponent } from '../../../shared/loading-text/loading-text';
import { ToastService } from '../../../core/toast/toast.service';
import { ConfirmDialogService } from '../../../core/confirm/confirm-dialog.service';

@Component({
  selector: 'app-reported-profile-detail-page',
  imports: [RouterLink, TranslatePipe, DatePipe, PlayerAvatarComponent, ErrorMessageComponent, LoadingTextComponent],
  templateUrl: './reported-profile-detail-page.html',
  styleUrl: './reported-profile-detail-page.css',
})
export class ReportedProfileDetailPage {
  private readonly route = inject(ActivatedRoute);
  private readonly moderationReviewService = inject(ModerationReviewService);
  private readonly toast = inject(ToastService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly translate = inject(TranslateService);

  protected readonly ReportStatus = ReportStatus;
  protected readonly reportReasonOptions = REPORT_REASON_OPTIONS;

  protected readonly detail = signal<ReportedProfileDetail | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isActionPending = signal(false);

  private readonly profileId: string;

  constructor() {
    this.profileId = this.route.snapshot.paramMap.get('profileId')!;
    this.loadDetail();
  }

  protected reasonLabel(reason: number): string {
    return this.reportReasonOptions.find((option) => option.value === reason)?.labelKey ?? '';
  }

  protected markReviewed(reportId: string): void {
    this.moderationReviewService.markReportReviewed(reportId).subscribe({
      next: () => this.loadDetail(),
      error: (error: HttpErrorResponse) => this.toast.error(extractErrorMessage(error)),
    });
  }

  protected async deletePost(postId: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      title: this.translate.instant('moderationReview.deletePostConfirmTitle'),
      message: this.translate.instant('moderationReview.deletePostConfirmMessage'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.isActionPending.set(true);
    this.moderationReviewService.deletePost(postId).subscribe({
      next: () => {
        this.isActionPending.set(false);
        this.loadDetail();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.isActionPending.set(false);
      },
    });
  }

  // Só a transição PRA banido pede confirmação — é a que tem consequência séria e
  // fácil de disparar sem querer (bloqueia login na hora). Desbanir é reversível
  // (banir de novo), não precisa do mesmo atrito.
  protected async toggleBan(): Promise<void> {
    const currentlyBanned = this.detail()?.isBanned ?? false;

    if (!currentlyBanned) {
      const confirmed = await this.confirmDialog.confirm({
        title: this.translate.instant('moderationReview.banConfirmTitle'),
        message: this.translate.instant('moderationReview.banConfirmMessage'),
        confirmLabel: this.translate.instant('moderationReview.banButton'),
        danger: true,
      });
      if (!confirmed) {
        return;
      }
    }

    this.isActionPending.set(true);

    const action$ = currentlyBanned
      ? this.moderationReviewService.unban(this.profileId)
      : this.moderationReviewService.ban(this.profileId);

    action$.subscribe({
      next: () => {
        this.isActionPending.set(false);
        this.loadDetail();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.isActionPending.set(false);
      },
    });
  }

  private loadDetail(): void {
    this.isLoading.set(true);
    this.moderationReviewService.getReportedProfileDetail(this.profileId).subscribe({
      next: (detail) => {
        this.detail.set(detail);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
