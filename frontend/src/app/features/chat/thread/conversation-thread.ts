import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ChatHubService } from '../../../core/chat/chat-hub.service';
import { ChatMessage } from '../../../core/chat/chat.models';
import { ChatService } from '../../../core/chat/chat.service';
import { PlayersService } from '../../../core/players/players.service';

const PAGE_SIZE = 30;

@Component({
  selector: 'app-conversation-thread',
  imports: [FormsModule, RouterLink, TranslatePipe],
  templateUrl: './conversation-thread.html',
  styleUrl: './conversation-thread.css',
})
export class ConversationThread {
  private readonly route = inject(ActivatedRoute);
  private readonly chatService = inject(ChatService);
  private readonly chatHubService = inject(ChatHubService);
  private readonly playersService = inject(PlayersService);

  private nextPage = 1;

  protected readonly conversationId = signal<string | null>(null);
  protected readonly otherProfileId = signal<string | null>(null);
  protected readonly otherDisplayName = signal('');
  protected readonly otherAvatarPath = signal<string | null>(null);

  protected readonly messages = signal<ChatMessage[]>([]);
  protected readonly hasMoreOlder = signal(false);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly draft = signal('');
  protected readonly isSending = signal(false);
  protected readonly sendErrorMessage = signal<string | null>(null);

  constructor() {
    const conversationIdParam = this.route.snapshot.paramMap.get('conversationId');
    const profileIdParam = this.route.snapshot.paramMap.get('profileId');

    if (conversationIdParam) {
      this.loadExistingConversation(conversationIdParam);
    } else if (profileIdParam) {
      this.loadFreshCompose(profileIdParam);
    }

    effect(() => {
      const incoming = this.chatHubService.receivedMessage();
      if (!incoming || incoming.conversationId !== this.conversationId()) {
        return;
      }

      this.appendMessage(incoming);
      this.chatHubService.markAsRead(incoming.conversationId);
    });
  }

  private loadExistingConversation(conversationId: string): void {
    this.conversationId.set(conversationId);
    this.isLoading.set(true);

    // A lista de conversas já traz nome/avatar do outro participante — reaproveitar evita
    // um endpoint novo só pra resolver "quem é o outro lado desta conversa".
    this.chatService.getConversations().subscribe({
      next: (conversations) => {
        const match = conversations.find((c) => c.conversationId === conversationId);
        if (match) {
          this.otherProfileId.set(match.otherProfileId);
          this.otherDisplayName.set(match.otherDisplayName);
          this.otherAvatarPath.set(match.otherAvatarPath);
        }
      },
    });

    this.chatService.getMessages(conversationId, 1, PAGE_SIZE).subscribe({
      next: (result) => {
        this.messages.set([...result.items].reverse());
        this.hasMoreOlder.set(result.items.length < result.totalCount);
        this.isLoading.set(false);
        this.chatHubService.markAsRead(conversationId);
      },
      error: () => {
        this.loadError.set('chat.loadError');
        this.isLoading.set(false);
      },
    });
  }

  private loadFreshCompose(profileId: string): void {
    this.otherProfileId.set(profileId);
    this.isLoading.set(true);

    this.chatService.getConversationWith(profileId).subscribe({
      next: (conversation) => {
        // Já existe conversa com essa pessoa — carrega o histórico normalmente.
        this.loadExistingConversation(conversation.conversationId);
      },
      error: () => {
        // Sem conversa ainda — só precisamos do nome/avatar pra montar o cabeçalho;
        // a conversa nasce no primeiro envio.
        this.playersService.getPlayerById(profileId).subscribe({
          next: (player) => {
            this.otherDisplayName.set(player.displayName);
            this.otherAvatarPath.set(player.avatarPath);
            this.isLoading.set(false);
          },
          error: () => {
            this.loadError.set('chat.loadError');
            this.isLoading.set(false);
          },
        });
      },
    });
  }

  protected loadOlderMessages(): void {
    const conversationId = this.conversationId();
    if (!conversationId) {
      return;
    }

    this.nextPage += 1;
    this.chatService.getMessages(conversationId, this.nextPage, PAGE_SIZE).subscribe({
      next: (result) => {
        this.messages.update((current) => [...[...result.items].reverse(), ...current]);
        this.hasMoreOlder.set(this.nextPage * PAGE_SIZE < result.totalCount);
      },
    });
  }

  protected async send(): Promise<void> {
    const content = this.draft().trim();
    const recipientId = this.otherProfileId();
    if (!content || !recipientId) {
      return;
    }

    this.isSending.set(true);
    this.sendErrorMessage.set(null);

    try {
      const sentMessage = await this.chatHubService.sendMessage(recipientId, content);
      this.conversationId.set(sentMessage.conversationId);
      this.appendMessage(sentMessage);
      this.draft.set('');
    } catch (error) {
      this.sendErrorMessage.set(this.extractHubErrorMessage(error));
    } finally {
      this.isSending.set(false);
    }
  }

  private appendMessage(message: ChatMessage): void {
    this.messages.update((current) => (current.some((m) => m.id === message.id) ? current : [...current, message]));
  }

  private extractHubErrorMessage(error: unknown): string {
    const raw = error instanceof Error ? error.message : String(error);
    const marker = 'HubException: ';
    const markerIndex = raw.indexOf(marker);
    return markerIndex >= 0 ? raw.slice(markerIndex + marker.length) : raw;
  }
}
