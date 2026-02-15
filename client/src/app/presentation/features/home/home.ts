import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { RouterLink } from '@angular/router';
import { 
  heroTrophy, 
  heroUsers, 
  heroSquares2x2, 
  heroCalendar,
  heroArrowRight,
  heroChartBar,
  heroHandRaised
} from '@ng-icons/heroicons/outline';

import { Auth } from '../../../core/services/auth/auth';
import { TournamentService } from '../../../core/services/competition/tournament.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NgIconComponent, RouterLink],
  viewProviders: [provideIcons({ 
    heroTrophy, heroUsers, heroSquares2x2, heroCalendar, heroArrowRight, heroChartBar , heroHandRaised
  })],
  templateUrl: './home.html',
})
export class Home implements OnInit {
  private tournamentService = inject(TournamentService);
  public auth = inject(Auth);
  private cdr = inject(ChangeDetectorRef);

  stats = {
    total: 0,
    live: 0,
    drafts: 0,
    players: 0 
  };

  user$ = this.auth.user$;

  ngOnInit() {
    this.loadDashboardStats();
  }

  loadDashboardStats() {
    this.tournamentService.getAllTournaments({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.succeeded && res.data) {
          this.stats.total = res.data.length;
          this.stats.live = res.data.filter(t => t.tournamentStateName === 'LIVE' || t.tournamentStateName === 'En Curso').length;
          this.stats.drafts = res.data.filter(t => t.tournamentStateName === 'DRFT' || t.tournamentStateName === 'Borrador').length;
          this.cdr.detectChanges();
        }
      }
    });
  }
}