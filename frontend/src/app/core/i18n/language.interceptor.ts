import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { LanguageService } from './language.service';

// Mesma ideia do authInterceptor: middleware do lado do cliente. Informa o
// backend, via Accept-Language, em qual idioma responder erros/validações
// (ver RequestLocalizationMiddleware no GtaConnect.Api).
//
// Restrito às chamadas da nossa API de propósito: o próprio TranslateHttpLoader
// usa HttpClient para buscar os JSONs de tradução (ex: /i18n/pt-BR.json), e esse
// fetch passa por TODOS os interceptors registrados. Sem esse filtro, injetar
// LanguageService aqui reentra na sua própria construção (LanguageService dispara
// o carregamento do idioma, que dispara essa requisição, que tenta injetar
// LanguageService de novo) — quebra o carregamento das traduções.
export const languageInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith(environment.apiUrl)) {
    return next(request);
  }

  const languageService = inject(LanguageService);
  const lang = languageService.currentLang() ?? 'pt-BR';

  const localizedRequest = request.clone({
    setHeaders: { 'Accept-Language': lang },
  });

  return next(localizedRequest);
};
