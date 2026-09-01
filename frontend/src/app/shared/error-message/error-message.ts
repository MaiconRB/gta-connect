import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// Padroniza o "bloco" de erro de carregamento de tela/formulário (rounded-lg bg-red-950...)
// que se repetia idêntico em 14 templates — só a margem externa variava (mb-3/mb-6/nenhuma),
// por isso `class` fica livre pra o caller passar. Não confundir com InlineErrorComponent
// (erro pontual de uma ação, texto pequeno sem card) nem com toast (ações fire-and-forget).
@Component({
  selector: 'app-error-message',
  imports: [],
  templateUrl: './error-message.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorMessageComponent {
  readonly message = input.required<string | null>();
  readonly class = input('');
}
