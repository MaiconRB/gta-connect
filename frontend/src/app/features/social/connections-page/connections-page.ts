import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { ConnectionStatus, ConnectionSummary } from '../../../core/social/social.models';
import { SocialService } from '../../../core/social/social.service';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';
import { LoadingTextComponent } from '../../../shared/loading-text/loading-text';

@Component({
  selector: 'app-connections-page',
  imports: [RouterLink, TranslatePipe, PlayerAvatarComponent, ErrorMessageComponent, LoadingTextComponent],
  templateUrl: './connections-page.html',
  styleUrl: './connections-page.css',
})
export class ConnectionsPage {
  private readonly socialService = inject(SocialService);

  protected readonly connections = signal<ConnectionSummary[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly processingId = signal<string | null>(null);

  protected readonly received = computed(() =>
    this.connections().filter((c) => c.status === ConnectionStatus.Pending && !c.isRequester),
  );
  protected readonly sent = computed(() =>
    this.connections().filter((c) => c.status === ConnectionStatus.Pending && c.isRequester),
  );
  protected readonly accepted = computed(() => this.connections().filter((c) => c.status === ConnectionStatus.Accepted));

  constructor() {
    this.loadConnections();
  }

  protected accept(connectionId: string): void {
    this.processingId.set(connectionId);
    this.socialService.accept(connectionId).subscribe({
      next: () => {
        this.processingId.set(null);
        this.loadConnections();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.processingId.set(null);
      },
    });
  }

  protected decline(connectionId: string): void {
    this.processingId.set(connectionId);
    this.socialService.decline(connectionId).subscribe({
      next: () => {
        this.processingId.set(null);
        this.loadConnections();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.processingId.set(null);
      },
    });
  }

  private loadConnections(): void {
    this.isLoading.set(true);
    this.socialService.getConnections().subscribe({
      next: (connections) => {
        this.connections.set(connections);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
