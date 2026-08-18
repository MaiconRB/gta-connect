import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { BlockedProfile } from '../../../core/moderation/moderation.models';
import { ModerationService } from '../../../core/moderation/moderation.service';

@Component({
  selector: 'app-blocked-list',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './blocked-list.html',
  styleUrl: './blocked-list.css',
})
export class BlockedList {
  private readonly moderationService = inject(ModerationService);

  protected readonly blockedProfiles = signal<BlockedProfile[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly unblockingId = signal<string | null>(null);

  constructor() {
    this.loadBlockedProfiles();
  }

  protected unblock(profileId: string): void {
    this.unblockingId.set(profileId);
    this.moderationService.unblock(profileId).subscribe({
      next: () => {
        this.blockedProfiles.update((current) => current.filter((p) => p.blockedProfileId !== profileId));
        this.unblockingId.set(null);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
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
