import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Validators } from '@angular/forms';

import { FormFieldConfig } from '../../../core/models/form-config';
import { DynamicForm } from '../../components/dynamic-form/dynamic-form';
import { PlayerService } from '../../../core/services/competition/player.service';
import { StatusModal } from "../../components/status-modal/status-modal";
import { NgIcon, provideIcons } from "@ng-icons/core";
import { heroArrowLeft, heroTrophy, heroXMark } from '@ng-icons/heroicons/outline';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-request-access',
  standalone: true,
  imports: [DynamicForm, CommonModule, StatusModal, NgIcon],
  viewProviders: [provideIcons({ heroXMark, heroTrophy, heroArrowLeft })],
  templateUrl: './request-access.html'
})
export class RequestAccess {

  private router = inject(Router);
  private playerService = inject(PlayerService);
  private titleService = inject(Title);


  showSuccessModal = false;
  isLoading = false;
  errorMessage: string | null = null;

  formFields: FormFieldConfig[] = [
    {
      name: 'nickname',
      label: 'Nickname',
      type: 'text',
      placeholder: 'Ej: PatoMaster',
      icon: 'user',
      validators: [Validators.required, Validators.minLength(3)]
    },
    {
      name: 'gameId',
      label: 'UID',
      type: 'text',
      placeholder: 'Ej: 123456789',
      icon: 'game',
      validators: [Validators.required]
    },
  ];

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.titleService.setTitle('PatoCup - Solicitud');
  }

  handleRequest(formData: any) {
  this.isLoading = true;
  this.errorMessage = null;

  this.playerService.publicSubmit(formData).subscribe({
    next: (response) => {
      this.isLoading = false;

      if (response.succeeded) {
        // Caso 1: Todo bien de verdad
        this.showSuccessModal = true; 
      } else {
        // Caso 2: El backend dice 'false' pero es una respuesta controlada
        // Si el mensaje dice "éxito", mostramos el modal verde
        if (response.message?.includes('éxito')) {
          this.showSuccessModal = true;
        } else {
          // Si es un error real (como el de "Ya existe una solicitud")
          this.errorMessage = response.message || 'La solicitud no pudo ser procesada.';
        }
      }
      this.cdr.detectChanges(); // Forzamos que el botón deje de decir "Procesando"
    },
    error: (err) => {
      // Caso 3: Error de red o error no controlado (400, 500, etc)
      this.isLoading = false;
      this.errorMessage = err.error?.message || 'Error de conexión con el servidor.';
      this.cdr.detectChanges();
    }
  });
}

  onSuccessFinish() {
    this.showSuccessModal = false;
    this.router.navigate(['/']); 
  }

  goBack() {
    this.router.navigate(['/']);
  }
}