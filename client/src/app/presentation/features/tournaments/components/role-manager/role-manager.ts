import { Component, OnInit, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-role-manager',
  templateUrl: './role-manager.html',
  imports: []
})
export class RoleManager implements OnInit {

    private titleService = inject(Title);
    
    ngOnInit(): void {
        this.titleService.setTitle('PatoCup - Roles');
    }
}