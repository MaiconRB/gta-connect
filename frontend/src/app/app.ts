import { DOCUMENT } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from './core/auth/auth.service';
import { ChatHubService } from './core/chat/chat-hub.service';
import { ChatService } from './core/chat/chat.service';
import { LanguageService } from './core/i18n/language.service';
import { LanguageSwitcher } from './shared/language-switcher/language-switcher';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, LanguageSwitcher, TranslatePipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  // Injetar aqui garante que o LanguageService (e a leitura do idioma salvo em
  // localStorage) roda assim que o app sobe, antes de qualquer tela renderizar.
  private readonly languageService = inject(LanguageService);
  private readonly document = inject(DOCUMENT);
  protected readonly authService = inject(AuthService);
  private readonly chatHubService = inject(ChatHubService);
  private readonly chatService = inject(ChatService);

  protected readonly unreadMessageCount = signal(0);

  constructor() {
    // Mantém <html lang="..."> em sincronia com o idioma ativo — importante pra
    // leitores de tela/acessibilidade, não é só um detalhe cosmético.
    effect(() => {
      const lang = this.languageService.currentLang();
      if (lang) {
        this.document.documentElement.lang = lang;
      }
    });

    // Conexão do chat vive aqui (não numa tela específica) pra receber mensagens de
    // qualquer lugar do app, não só com a conversa aberta — é o que alimenta o badge.
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.chatHubService.connect();
        this.refreshUnreadCount();
      } else {
        this.chatHubService.disconnect();
        this.unreadMessageCount.set(0);
      }
    });

    effect(() => {
      if (this.chatHubService.receivedMessage()) {
        this.refreshUnreadCount();
      }
    });

    // Segundo gatilho, independente do de cima: receivedMessage e o MarkAsRead da conversa
    // aberta reagem ao mesmo sinal, sem ordem garantida entre o GET daqui e o invoke de lá —
    // esse aqui só dispara DEPOIS que o servidor confirma o MarkAsRead, garantindo que o
    // badge reflita o estado já persistido (não uma leitura que corre na frente da escrita).
    effect(() => {
      if (this.chatHubService.conversationMarkedAsRead()) {
        this.refreshUnreadCount();
      }
    });
  }

  private refreshUnreadCount(): void {
    this.chatService.getConversations().subscribe({
      next: (conversations) => this.unreadMessageCount.set(conversations.reduce((sum, c) => sum + c.unreadCount, 0)),
    });
  }
}
