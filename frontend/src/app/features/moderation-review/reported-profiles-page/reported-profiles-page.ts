import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { ModerationReviewService } from '../../../core/moderation-review/moderation-review.service';
import { ReportedProfileSummary } from '../../../core/moderation-review/moderation-review.models';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';
import { LoadingTextComponent } from '../../../shared/loading-text/loading-text';

@Component({
  selector: 'app-reported-profiles-page',
  imports: [RouterLink, TranslatePipe, PlayerAvatarComponent, ErrorMessageComponent, LoadingTextComponent],
  templateUrl: './reported-profiles-page.html',
  styleUrl: './reported-profiles-page.css',
})
export class ReportedProfilesPage {
  private readonly moderationReviewService = inject(ModerationReviewService);

  protected readonly profiles = signal<ReportedProfileSummary[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.moderationReviewService.getReportedProfiles().subscribe({
      next: (profiles) => {
        this.profiles.set(profiles);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
