import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

// Guard funcional (padrão Angular 15+, substitui a antiga classe CanActivate).
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated() ? true : router.parseUrl('/login');
};

// Login/registro não fazem sentido com sessão já aberta — manda pra busca,
// que é a tela principal do produto depois de autenticar.
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated() ? router.parseUrl('/jogadores') : true;
};

// A proteção real é no backend ([Authorize(Roles="Moderator")]) — este guard só evita
// a tela piscar pra quem não tem acesso. Redireciona pro perfil (não pro login — já
// está autenticado, só não é moderador).
export const moderatorGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isModerator() ? true : router.parseUrl('/perfil');
};
