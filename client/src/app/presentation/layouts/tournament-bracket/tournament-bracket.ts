import { Component, OnInit, OnDestroy, inject, HostListener, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgIconComponent, provideIcons } from "@ng-icons/core";
import { heroTrophy, heroArrowLeft, heroStar, heroInformationCircle } from '@ng-icons/heroicons/outline';
import { TournamentService } from '../../../core/services/competition/tournament.service';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-tournament-bracket',
  standalone: true,
  imports: [CommonModule, NgIconComponent],
  viewProviders: [provideIcons({ heroTrophy, heroArrowLeft, heroStar, heroInformationCircle })],
  templateUrl: './tournament-bracket.html'
})
export class TournamentBracket implements OnInit, OnDestroy {
  private router = inject(Router);
  private tournamentService = inject(TournamentService);
  private cdr = inject(ChangeDetectorRef);

  tournament: any = null;
  isLoading = true;
  orderedPhases: any[] = [];
  finalPhase: any = null;
  winnerName: string = 'POR DEFINIR';

  private pollingSub?: Subscription;

  ngOnInit() {
    this.loadTournamentData();
    this.pollingSub = interval(10000).subscribe(() => this.fetchBracket(true));
  }

  ngOnDestroy() {
    this.pollingSub?.unsubscribe();
  }

  loadTournamentData() {
    this.isLoading = true;
    this.fetchBracket();
  }

  private fetchBracket(isSilent = false) {
    this.tournamentService.getPublicBracket().subscribe({
      next: (res) => {
        if (res.succeeded && res.data) {
          this.tournament = res.data;
          this.organizeLinearTournament(res.data);
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  organizeLinearTournament(data: any) {
    const phasesArray = Array.isArray(data) ? data : (data.phases || []);

    if (phasesArray.length === 0) {
      this.winnerName = 'POR DEFINIR';
      return;
    }

    this.orderedPhases = [...phasesArray].sort((a, b) => a.phaseOrder - b.phaseOrder);

    this.finalPhase = phasesArray.find((p: any) => p.isFinal === true);

    if (this.finalPhase && this.finalPhase.matches && this.finalPhase.matches.length > 0) {
      const match = this.finalPhase.matches[0];

      if (match.scorePlayer1 > match.scorePlayer2) {
        this.winnerName = match.player1Name;
      } else if (match.scorePlayer2 > match.scorePlayer1) {
        this.winnerName = match.player2Name;
      } else if (match.scorePlayer1 === 0 && match.scorePlayer2 === 0) {
        this.winnerName = 'SIN RESULTADO';
      } else {
        this.winnerName = 'DEFINIENDO...';
      }
    } else {
      this.winnerName = 'POR DEFINIR';
    }
  }

  @HostListener('window:keydown.escape')
  goHome() {
    this.router.navigate(['/']);
  }
}