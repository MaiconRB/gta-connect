import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// Ícones de UI (usados hoje em app.html, centralizados aqui pra matar a duplicação
// desktop/mobile) + ícones temáticos de conceitos genéricos do gênero GTA (carro, cofre,
// mira, etc — usados nas tags de estilo de jogo). Nenhum reproduz logo/HUD oficial da
// Rockstar (ver PROJETO.md §7 e o plano da Fase 4 — decisão consciente de manter só a
// "vibe", não elementos reconhecíveis).
export type IconName =
  | 'profile'
  | 'search'
  | 'feed'
  | 'connections'
  | 'messages'
  | 'bell'
  | 'shield'
  | 'warning'
  | 'vault'
  | 'car'
  | 'crosshair'
  | 'mask'
  | 'users'
  | 'map'
  | 'briefcase';

export type IconSize = 'sm' | 'md';

@Component({
  selector: 'app-icon',
  imports: [],
  templateUrl: './icon.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconComponent {
  readonly name = input.required<IconName>();
  readonly size = input<IconSize>('md');
}
