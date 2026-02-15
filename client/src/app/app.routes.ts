import { Routes } from '@angular/router';
import { Login } from './presentation/features/auth/login/login';
import { AuthLayout } from './presentation/layouts/auth-layout/auth-layout';
import { MainLayout } from './presentation/layouts/main-layout/main-layout';
import { Home } from './presentation/features/home/home';
import { authGuard } from './core/guards/auth-guard';
import { Landing } from './presentation/layouts/landing/landing';
import { RequestAccess } from './presentation/layouts/request-access/request-access';
import { publicGuard } from './core/guards/public.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [publicGuard],
    component: Landing,
    pathMatch: 'full'
  },
  {
    path: 'auth',
    component: AuthLayout,
    canActivate: [publicGuard],
    children: [
      { path: 'login', component: Login },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },
  {
    path: 'public',
    children: [
      { path: 'request', component: RequestAccess }
    ]
  },
  {
    path: 'tournament',
    loadComponent: () => import('./presentation/layouts/tournament-bracket/tournament-bracket')
      .then(m => m.TournamentBracket)
  },

  // Rutas Protegidas 
  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    children: [
      { path: 'home', component: Home },

      {
        path: 'competition',
        children: [
          {
            path: 'tournaments',
            loadChildren: () => import('./presentation/features/tournaments/tournament.routes')
              .then(m => m.tournamentRoutes)
          },
          {
            path: 'players',
            loadComponent: () => import('./presentation/features/tournaments/components/player-manager/player-manager')
              .then(m => m.PlayerManager)
          }
        ]
      },
      {
        path: 'security',
        children: [
          {
            path: 'users',
            loadComponent: () => import('./presentation/features/tournaments/components/user-manager/user-manager')
              .then(m => m.UserManager)
          },
          {
            path: 'roles',
            loadComponent: () => import('./presentation/features/tournaments/components/role-manager/role-manager')
              .then(m => m.RoleManager)
          },
          {
            path: 'audit',
            loadComponent: () => import('./presentation/features/tournaments/components/audit-manager/audit-manager')
              .then(m => m.AuditManager)
          },
        ]
      }
    ]
  },

  { path: '**', redirectTo: '' }
];