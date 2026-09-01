import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// Erro pontual de uma ação/campo específico, sem o card do ErrorMessageComponent — usado
// onde o erro precisa ficar visualmente perto do controle que o causou (ex: erro de upload
// de avatar, ao lado do file-picker). Ações "fire and forget" (bloquear, banir, avaliar...)
// usam ToastService em vez disso — ver plano da Fase 4.
@Component({
  selector: 'app-inline-error',
  imports: [],
  templateUrl: './inline-error.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InlineErrorComponent {
  readonly message = input.required<string | null>();
  readonly class = input('');
}
