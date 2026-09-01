import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

// Texto de loading padrão, 100% idêntico hoje em ~10 telas (mesma classe, mesma chave i18n
// `profile.loading` reaproveitada globalmente). Zero inputs de propósito — se um dia uma
// tela precisar de texto diferente, é sinal pra adicionar um `key` input então, não agora.
@Component({
  selector: 'app-loading-text',
  imports: [TranslatePipe],
  templateUrl: './loading-text.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingTextComponent {}
