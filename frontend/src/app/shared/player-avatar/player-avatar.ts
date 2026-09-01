import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type PlayerAvatarSize = 'sm' | 'md' | 'lg';

interface SizeClasses {
  wrapper: string;
  fallbackText: string;
}

// Mata a duplicação do bloco "avatar com fallback de inicial" repetido em 11 templates
// (chat, feed, moderação, notificações, player-detail/search, profile, connections) —
// cada um usava nomes de campo diferentes (avatarPath/displayName, otherAvatarPath/
// otherDisplayName, authorAvatarPath/authorDisplayName, actorAvatarPath/actorDisplayName),
// por isso os Inputs aqui são primitivos, não um objeto — evita forçar um adapter em
// cada call site só pra satisfazer um shape comum.
@Component({
  selector: 'app-player-avatar',
  imports: [],
  templateUrl: './player-avatar.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerAvatarComponent {
  readonly src = input<string | null | undefined>(null);
  readonly displayName = input.required<string>();
  readonly size = input<PlayerAvatarSize>('md');
  readonly online = input(false);
  readonly lazy = input(false);

  // sm=w-10 (linhas densas: chat, feed, notificações), md=w-12 (linhas de lista padrão),
  // lg=w-16 (avatar hero: player-detail, profile, detalhe de denúncia) — mapeamento fixo
  // pra classe Tailwind, não geração dinâmica de tamanho (evita classe não purgada).
  protected readonly sizeClasses = computed<SizeClasses>(() => {
    switch (this.size()) {
      case 'sm':
        return { wrapper: 'w-10 h-10', fallbackText: 'font-semibold' };
      case 'lg':
        return { wrapper: 'w-16 h-16', fallbackText: 'text-xl font-semibold' };
      default:
        return { wrapper: 'w-12 h-12', fallbackText: 'font-semibold' };
    }
  });

  protected readonly initial = computed(() => this.displayName().charAt(0).toUpperCase());
}
