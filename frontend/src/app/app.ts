import { DOCUMENT } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from './core/auth/auth.service';
import { ChatHubService } from './core/chat/chat-hub.service';
import { LanguageService } from './core/i18n/language.service';
import { NotificationsService } from './core/notifications/notifications.service';
import { LanguageSwitcher } from './shared/language-switcher/language-switcher';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LanguageSwitcher, TranslatePipe],
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
  private readonly notificationsService = inject(NotificationsService);

  // Sino único no header substitui os badges avulsos que existiam antes (mensagens não
  // lidas, pedidos de conexão pendentes) — centro de notificações cobre os dois casos
  // (e mais avaliação/curtida) numa contagem só.
  protected readonly unreadNotificationCount = signal(0);
  protected readonly isEmailConfirmed = this.authService.isEmailConfirmed;
  protected readonly resendingConfirmation = signal(false);
  protected readonly resendSuccess = signal(false);

  constructor() {
    // Mantém <html lang="..."> em sincronia com o idioma ativo — importante pra
    // leitores de tela/acessibilidade, não é só um detalhe cosmético.
    effect(() => {
      const lang = this.languageService.currentLang();
      if (lang) {
        this.document.documentElement.lang = lang;
      }
    });

    // Conexão do chat vive aqui (não numa tela específica) — precisa ficar aberta o tempo
    // todo tanto pra mensagens em tempo real quanto pras notificações (mesma conexão,
    // evento SignalR diferente: ReceiveMessage vs ReceiveNotification).
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.chatHubService.connect();
        this.refreshUnreadNotificationCount();
      } else {
        this.chatHubService.disconnect();
        this.unreadNotificationCount.set(0);
      }
    });

    effect(() => {
      if (this.chatHubService.receivedNotification()) {
        this.refreshUnreadNotificationCount();
      }
    });

    // A notifications-page chama notificationsService.notifyChanged() depois de marcar
    // como lida (individual ou em lote) — sem isso o sino ficaria desatualizado até a
    // próxima notificação chegar em tempo real.
    effect(() => {
      if (this.notificationsService.notificationsChanged() > 0) {
        this.refreshUnreadNotificationCount();
      }
    });
  }

  private refreshUnreadNotificationCount(): void {
    this.notificationsService.getUnreadCount().subscribe({
      next: (count) => this.unreadNotificationCount.set(count),
    });
  }

  protected resendConfirmationEmail(): void {
    if (this.resendingConfirmation()) return;
    this.resendingConfirmation.set(true);
    this.authService.resendConfirmation().subscribe({
      next: () => {
        this.resendSuccess.set(true);
        this.resendingConfirmation.set(false);
      },
      error: () => {
        this.resendingConfirmation.set(false);
      },
    });
  }
}
