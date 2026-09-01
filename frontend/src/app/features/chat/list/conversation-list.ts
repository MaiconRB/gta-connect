import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ChatHubService } from '../../../core/chat/chat-hub.service';
import { ConversationSummary } from '../../../core/chat/chat.models';
import { ChatService } from '../../../core/chat/chat.service';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';
import { LoadingTextComponent } from '../../../shared/loading-text/loading-text';

@Component({
  selector: 'app-conversation-list',
  imports: [RouterLink, TranslatePipe, PlayerAvatarComponent, ErrorMessageComponent, LoadingTextComponent],
  templateUrl: './conversation-list.html',
  styleUrl: './conversation-list.css',
})
export class ConversationList {
  private readonly chatService = inject(ChatService);
  private readonly chatHubService = inject(ChatHubService);

  protected readonly conversations = signal<ConversationSummary[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.loadConversations();

    // Nova mensagem chegando (de qualquer conversa) — recarrega a lista pra atualizar
    // prévia/ordem/badge de não-lidas, sem precisar reconciliar estado manualmente.
    effect(() => {
      if (this.chatHubService.receivedMessage()) {
        this.loadConversations();
      }
    });

    // Segundo gatilho: um MarkAsRead concluído (de uma conversa aberta em outra tela) pode
    // acontecer depois do GET disparado acima — sem isso, a lista ficaria com um badge
    // desatualizado até a próxima mensagem chegar.
    effect(() => {
      if (this.chatHubService.conversationMarkedAsRead()) {
        this.loadConversations();
      }
    });
  }

  private loadConversations(): void {
    this.isLoading.set(true);
    this.chatService.getConversations().subscribe({
      next: (conversations) => {
        this.conversations.set(conversations);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
