import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { AppNotification } from '../notifications/notifications.models';
import { ChatMessage } from './chat.models';

// Conexão única do Hub pro app inteiro (providedIn: 'root') — iniciada/parada a partir de
// App, reagindo a authService.isAuthenticated(), pra receber mensagens em qualquer tela,
// não só na conversa aberta (necessário pro badge de não-lidas na navegação).
@Injectable({ providedIn: 'root' })
export class ChatHubService {
  private readonly authService = inject(AuthService);
  private connection: signalR.HubConnection | null = null;

  // Sinal simples: cada nova mensagem publica um valor novo, componentes reagem via effect().
  readonly receivedMessage = signal<ChatMessage | null>(null);

  // Só vira valor DEPOIS que o servidor confirma o MarkAsRead (não junto com receivedMessage) —
  // App usa isso pra recalcular o badge de não-lidas sem correr risco de ler o banco antes da
  // escrita do MarkAsRead commitar (as duas coisas reagem ao mesmo receivedMessage, mas o GET
  // de contagem e o invoke de MarkAsRead são requisições independentes, sem ordem garantida).
  // Objeto novo a cada chamada (não a string pura) — sinal com o MESMO valor primitivo em
  // sequência (ex: mesma conversa marcada como lida duas vezes seguidas) não dispara effects
  // no Angular por igualdade de valor; um objeto literal sempre conta como mudança.
  readonly conversationMarkedAsRead = signal<{ conversationId: string } | null>(null);

  // Notificações in-app reaproveitam esta mesma conexão (já fica aberta o tempo todo
  // enquanto o usuário está logado) — sem hub/WebSocket segundo só pra isso.
  readonly receivedNotification = signal<AppNotification | null>(null);

  connect(): void {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.chatHubUrl, { accessTokenFactory: () => this.authService.getToken() ?? '' })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceiveMessage', (message: ChatMessage) => this.receivedMessage.set(message));
    this.connection.on('ReceiveNotification', (notification: AppNotification) => this.receivedNotification.set(notification));

    this.connection.start().catch((error) => console.error('Falha ao conectar no chat:', error));
  }

  disconnect(): void {
    this.connection?.stop();
    this.connection = null;
  }

  // Retorna a mensagem persistida (com o conversationId real) — quem chama usa isso pra
  // saber, na primeira mensagem de uma conversa nova, qual conversationId acabou de nascer.
  sendMessage(recipientProfileId: string, content: string): Promise<ChatMessage> {
    return this.connection!.invoke<ChatMessage>('SendMessage', recipientProfileId, content);
  }

  async markAsRead(conversationId: string): Promise<void> {
    await this.connection?.invoke('MarkAsRead', conversationId);
    this.conversationMarkedAsRead.set({ conversationId });
  }
}
