import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from '../../components/navbar/navbar';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, Navbar],
  templateUrl: './main-layout.html'
})
export class MainLayout implements OnInit{

  private titleService = inject(Title);

  ngOnInit() {
    this.titleService.setTitle('PatoCup - Inicio');
  }

}