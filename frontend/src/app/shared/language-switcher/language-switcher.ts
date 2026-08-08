import { Component, inject } from '@angular/core';
import { LanguageService, SUPPORTED_LANGUAGES, SupportedLanguage } from '../../core/i18n/language.service';

@Component({
  selector: 'app-language-switcher',
  imports: [],
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.css',
})
export class LanguageSwitcher {
  private readonly languageService = inject(LanguageService);

  protected readonly languages = SUPPORTED_LANGUAGES;
  protected readonly currentLang = this.languageService.currentLang;

  protected select(lang: SupportedLanguage): void {
    this.languageService.setLanguage(lang);
  }
}
