import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

const STORAGE_KEY = 'gta-connect:lang';
export const SUPPORTED_LANGUAGES = ['pt-BR', 'en'] as const;
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translateService = inject(TranslateService);

  // Repassa o signal do TranslateService — qualquer componente que leia `currentLang`
  // reage automaticamente quando o idioma muda, sem precisar de Subject manual.
  readonly currentLang = this.translateService.currentLang;

  constructor() {
    const storedLang = localStorage.getItem(STORAGE_KEY);
    const initialLang = this.isSupported(storedLang) ? storedLang : 'pt-BR';

    // Chama use() sempre, mesmo quando initialLang já bate com o config inicial
    // (provideTranslateService({ lang: 'pt-BR' })) — esse config só define o nome
    // do idioma corrente, ele NÃO garante que o loader HTTP já buscou o JSON.
    // Sem essa chamada explícita, o carregamento pode nunca disparar.
    this.translateService.use(initialLang).subscribe();
  }

  setLanguage(lang: SupportedLanguage): void {
    this.translateService.use(lang).subscribe();
    localStorage.setItem(STORAGE_KEY, lang);
  }

  private isSupported(lang: string | null): lang is SupportedLanguage {
    return SUPPORTED_LANGUAGES.includes(lang as SupportedLanguage);
  }
}
