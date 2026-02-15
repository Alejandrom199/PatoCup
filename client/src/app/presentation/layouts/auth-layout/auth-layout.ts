import { Component } from '@angular/core';
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

  constructor(private router: Router) {

  }
  goBack() {
    this.router.navigate(['/']);
  }

}