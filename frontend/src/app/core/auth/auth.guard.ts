import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

// Guard funcional (padrão Angular 15+, substitui a antiga classe CanActivate).
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated() ? true : router.parseUrl('/login');
};

// A proteção real é no backend ([Authorize(Roles="Moderator")]) — este guard só evita
// a tela piscar pra quem não tem acesso. Redireciona pro perfil (não pro login — já
// está autenticado, só não é moderador).
export const moderatorGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isModerator() ? true : router.parseUrl('/perfil');
};
