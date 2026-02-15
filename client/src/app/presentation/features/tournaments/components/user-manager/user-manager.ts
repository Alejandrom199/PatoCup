import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef, ViewChild, ElementRef } from '@angular/core';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-manager',
  standalone: true,
  imports: [],
  templateUrl: './user-manager.html',
})
export class UserManager implements OnInit {
  ngOnInit() {
  }
}