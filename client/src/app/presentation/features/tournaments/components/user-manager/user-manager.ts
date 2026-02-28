import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef, ViewChild, ElementRef, inject } from '@angular/core';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-user-manager',
  standalone: true,
  imports: [],
  templateUrl: './user-manager.html',
})
export class UserManager implements OnInit {
  private titleService = inject(Title);
  ngOnInit() {
    this.titleService.setTitle('PatoCup - Usuarios');
  }
}