import { AfterViewInit, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NgIconComponent, provideIcons } from "@ng-icons/core";
// Importamos los iconos necesarios
import { heroTrophy, heroUserPlus, heroLockClosed } from '@ng-icons/heroicons/outline';
import { TournamentBracket } from '../tournament-bracket/tournament-bracket'; // Ajusta la ruta
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [NgIconComponent, TournamentBracket],
  viewProviders: [provideIcons({ heroTrophy, heroUserPlus, heroLockClosed })],
  templateUrl: './landing.html',
})
export class Landing implements AfterViewInit {
  @ViewChild('video') video!: ElementRef<HTMLVideoElement>;
  showBrackets = false;

  private titleService = inject(Title);

  constructor(private router: Router) {}

  ngOnInit() {
    this.titleService.setTitle('PatoCup - Inicio');
  }
  
  ngAfterViewInit(): void {
    const v = this.video.nativeElement;
    v.volume = 0.5;
  }

  goToLogin() { this.router.navigate(['/auth/login']); }
  goToRequest() { this.router.navigate(['/public/request']); }
  
  openBrackets() {
    this.router.navigate(['/tournament']);
  }
  closeBrackets() { this.showBrackets = false; }
}