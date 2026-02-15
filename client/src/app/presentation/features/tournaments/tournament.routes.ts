import { Routes } from '@angular/router';

export const tournamentRoutes: Routes = [
    {
        path: '', 
        loadComponent: () => import('./components/tournament-manager/tournament-manager')
            .then(m => m.TournamentManager)
    },
    {
        path: ':id/details',
        loadComponent: () => import('./pages/tournament-info/tournament-info')
            .then(m => m.TournamentInfo)
    },
];