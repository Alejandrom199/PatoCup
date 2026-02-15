import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterModule } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroArrowRightOnRectangle,
  heroSquares2x2,
  heroUser,
  heroUsers,
  heroTrophy,
  heroChevronDown,
  heroCog6Tooth,
  heroLockClosed,
  heroShieldCheck,
  
} from '@ng-icons/heroicons/outline';
import { Auth } from '../../../core/services/auth/auth';
import { MenuService } from '../../../core/services/security/menu.service';
import { GroupedMenu } from '../../../data/features/tournaments/dtos/security/menu.dto';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, NgIconComponent, RouterModule],
  viewProviders: [provideIcons({
    heroArrowRightOnRectangle,
    heroSquares2x2,
    heroUser,
    heroUsers,
    heroTrophy,
    heroChevronDown,
    heroCog6Tooth,
    heroLockClosed,
    heroShieldCheck
  })],
  templateUrl: './navbar.html'
})
export class Navbar implements OnInit {
  public auth = inject(Auth);
  private router = inject(Router);
  private menuService = inject(MenuService);

  menuGroups$!: Observable<GroupedMenu[]>;

  ngOnInit() {
    this.menuGroups$ = this.menuService.getMyMenu();
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/']).then(() => window.location.reload());
  }
}
