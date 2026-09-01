import { Injectable, signal } from '@angular/core';

export type ToastVariant = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  variant: ToastVariant;
  message: string;
}

// Substitui os ~9 signals de erro dedicados (um por ação: bloquear, banir, avaliar,
// denunciar, apagar post, marcar como lida, enviar mensagem...) por um único canal de
// feedback efêmero — ver plano da Fase 4 (§3) pra critério de quando usar toast vs erro
// inline (formulários continuam inline, ações pontuais viram toast).
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;

  readonly toasts = signal<Toast[]>([]);

  success(message: string, durationMs = 4000): void {
    this.push('success', message, durationMs);
  }

  // Erro fica mais tempo na tela — é a mensagem que o usuário mais precisa ter tempo de ler.
  error(message: string, durationMs = 6000): void {
    this.push('error', message, durationMs);
  }

  info(message: string, durationMs = 4000): void {
    this.push('info', message, durationMs);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private push(variant: ToastVariant, message: string, durationMs: number): void {
    const id = this.nextId++;
    this.toasts.update((list) => [...list, { id, variant, message }]);
    setTimeout(() => this.dismiss(id), durationMs);
  }
}
