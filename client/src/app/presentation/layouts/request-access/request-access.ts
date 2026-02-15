import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Validators } from '@angular/forms';

import { FormFieldConfig } from '../../../core/models/form-config';
import { DynamicForm } from '../../components/dynamic-form/dynamic-form';
import { PlayerService } from '../../../core/services/competition/player.service';
import { StatusModal } from "../../components/status-modal/status-modal";
import { NgIcon, provideIcons } from "@ng-icons/core";
import { heroArrowLeft, heroTrophy, heroXMark } from '@ng-icons/heroicons/outline';

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

  handleRequest(formData: any) {
    this.isLoading = true;
    this.errorMessage = null;

    this.playerService.publicSubmit(formData).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.succeeded) {
          this.showSuccessModal = true; 
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Error al enviar la solicitud.';
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