import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { REPORT_REASON_OPTIONS } from '../../../core/moderation/moderation.models';
import { ModerationReviewService } from '../../../core/moderation-review/moderation-review.service';
import { ReportedProfileDetail, ReportStatus } from '../../../core/moderation-review/moderation-review.models';

@Component({
  selector: 'app-reported-profile-detail-page',
  imports: [RouterLink, TranslatePipe, DatePipe],
  templateUrl: './reported-profile-detail-page.html',
  styleUrl: './reported-profile-detail-page.css',
})
export class ReportedProfileDetailPage {
  private readonly route = inject(ActivatedRoute);
  private readonly moderationReviewService = inject(ModerationReviewService);

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
      error: (error: HttpErrorResponse) => this.errorMessage.set(extractErrorMessage(error)),
    });
  }

  protected deletePost(postId: string): void {
    this.isActionPending.set(true);
    this.moderationReviewService.deletePost(postId).subscribe({
      next: () => {
        this.isActionPending.set(false);
        this.loadDetail();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isActionPending.set(false);
      },
    });
  }

  protected toggleBan(): void {
    const currentlyBanned = this.detail()?.isBanned ?? false;
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
        this.errorMessage.set(extractErrorMessage(error));
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
