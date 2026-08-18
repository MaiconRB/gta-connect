import { DOCUMENT } from '@angular/common';
import { Component, effect, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from './core/auth/auth.service';
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

  constructor() {
    // Mantém <html lang="..."> em sincronia com o idioma ativo — importante pra
    // leitores de tela/acessibilidade, não é só um detalhe cosmético.
    effect(() => {
      const lang = this.languageService.currentLang();
      if (lang) {
        this.document.documentElement.lang = lang;
      }
    });
  }
}
