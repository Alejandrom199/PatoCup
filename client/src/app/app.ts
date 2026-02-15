import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StatusModal } from './presentation/components/status-modal/status-modal';
import { StatusService } from './core/services/status/status.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, StatusModal],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('patocup-front');

  constructor(public statusService: StatusService) {}
}
