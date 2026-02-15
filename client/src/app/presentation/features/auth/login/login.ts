import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Validators } from '@angular/forms';

import { FormFieldConfig } from '../../../../core/models/form-config';
import { DynamicForm } from '../../../components/dynamic-form/dynamic-form';
import { Auth } from '../../../../core/services/auth/auth';
import { StatusService } from '../../../../core/services/status/status.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, DynamicForm, RouterModule],
  templateUrl: './login.html'
})
export class Login {
  private auth = inject(Auth);
  private router = inject(Router);
  private statusService = inject(StatusService);
  private cdr = inject(ChangeDetectorRef);

  isLoading = false;
  errorMessage: string | null = null;

  loginFields: FormFieldConfig[] = [
    {
      name: 'username',
      label: 'Usuario',
      type: 'text',
      placeholder: 'Ej: admin',
      icon: 'user',
      validators: [Validators.required]
    },
    {
      name: 'password',
      label: 'Contraseña',
      type: 'password',
      placeholder: '••••••••••',
      icon: 'lock',
      validators: [Validators.required]
    },
  ];

  handleLogin(formData: any) {
    this.isLoading = true;
    this.errorMessage = null;

    this.auth.login(formData).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.statusService.show(true, response.message || '¡Bienvenido!');
        this.router.navigate(['/home']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Error de autenticación';
        this.statusService.show(false, this.errorMessage!);
        this.cdr.detectChanges();
      }
    });
  }
}