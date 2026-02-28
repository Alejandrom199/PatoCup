import { Component, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Router, RouterOutlet } from '@angular/router';
import { provideIcons, NgIcon } from '@ng-icons/core';
import { heroUserCircleSolid } from '@ng-icons/heroicons/solid';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet],
  viewProviders: [provideIcons({ heroUserCircleSolid })],
  templateUrl: './auth-layout.html'
})
export class AuthLayout {

  private titleService = inject(Title);
  constructor(private router: Router) {

  }

  ngOnInit() {
    this.titleService.setTitle('PatoCup - Autenticación');
  }

  goBack() {
    this.router.navigate(['/']);
  }

}