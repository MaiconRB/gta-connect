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
  {
    path: 'mensagens',
    loadComponent: () => import('./features/chat/list/conversation-list').then((m) => m.ConversationList),
    canActivate: [authGuard],
  },
  {
    path: 'mensagens/with/:profileId',
    loadComponent: () => import('./features/chat/thread/conversation-thread').then((m) => m.ConversationThread),
    canActivate: [authGuard],
  },
  {
    path: 'mensagens/:conversationId',
    loadComponent: () => import('./features/chat/thread/conversation-thread').then((m) => m.ConversationThread),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: 'login' },
];
