import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { BlockedProfile } from '../../../core/moderation/moderation.models';
import { ModerationService } from '../../../core/moderation/moderation.service';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';
import { LoadingTextComponent } from '../../../shared/loading-text/loading-text';
import { ToastService } from '../../../core/toast/toast.service';
import { ConfirmDialogService } from '../../../core/confirm/confirm-dialog.service';

@Component({
  selector: 'app-blocked-list',
  imports: [RouterLink, TranslatePipe, PlayerAvatarComponent, ErrorMessageComponent, LoadingTextComponent],
  templateUrl: './blocked-list.html',
  styleUrl: './blocked-list.css',
})
export class BlockedList {
  private readonly moderationService = inject(ModerationService);
  private readonly toast = inject(ToastService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly translate = inject(TranslateService);

  protected readonly blockedProfiles = signal<BlockedProfile[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly unblockingId = signal<string | null>(null);

  constructor() {
    this.loadBlockedProfiles();
  }

  protected async unblock(profileId: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      title: this.translate.instant('moderation.unblockConfirmTitle'),
      message: this.translate.instant('moderation.unblockConfirmMessage'),
    });
    if (!confirmed) {
      return;
    }

    this.unblockingId.set(profileId);
    this.moderationService.unblock(profileId).subscribe({
      next: () => {
        this.blockedProfiles.update((current) => current.filter((p) => p.blockedProfileId !== profileId));
        this.unblockingId.set(null);
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.unblockingId.set(null);
      },
    });
  }

  private loadBlockedProfiles(): void {
    this.isLoading.set(true);
    this.moderationService.getBlockedProfiles().subscribe({
      next: (profiles) => {
        this.blockedProfiles.set(profiles);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
