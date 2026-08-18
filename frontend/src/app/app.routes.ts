import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'registrar',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'perfil',
    loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
    canActivate: [authGuard],
  },
  {
    path: 'jogadores',
    loadComponent: () => import('./features/players/search/player-search').then((m) => m.PlayerSearch),
    canActivate: [authGuard],
  },
  {
    path: 'jogadores/:id',
    loadComponent: () => import('./features/players/detail/player-detail').then((m) => m.PlayerDetail),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: 'login' },
];
