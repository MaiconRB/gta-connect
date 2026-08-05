import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

// Interceptor funcional (padrão Angular 15+, substitui a antiga classe HttpInterceptor).
// Registrado uma vez em app.config.ts — todo HttpClient.request() passa por aqui.
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (!token) {
    return next(request);
  }

  const authorizedRequest = request.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });

  return next(authorizedRequest);
};
