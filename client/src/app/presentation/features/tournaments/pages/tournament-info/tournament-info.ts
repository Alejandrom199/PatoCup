import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core'; // 1. IMPORTAR ChangeDetectorRef
import { ActivatedRoute, RouterModule } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroTrophy, heroCalendar, heroMapPin, heroPencil, heroArrowRightOnRectangle, heroCheckBadge, heroGlobeAlt } from '@ng-icons/heroicons/outline';

import { PhasesManager } from '../../components/phases-manager/phases-manager';
import { TournamentResponseDto, UpdateTournamentDto } from '../../../../../data/features/tournaments/dtos/competition/tournament.dto';
import { Form, Validators } from '@angular/forms';
import { FormFieldConfig } from '../../../../../core/models/form-config';
import { DynamicForm } from "../../../../components/dynamic-form/dynamic-form";
import { Catalog } from '../../../../../core/models/catalog'
import { CatalogService } from '../../../../../core/services/common/catalog.service';
import { TournamentService } from '../../../../../core/services/competition/tournament.service';
import { PhaseService } from '../../../../../core/services/competition/phase.service';

interface TournamentUI extends TournamentResponseDto {
  isExpanded?: boolean;
  isPublic?: boolean;
  phases: any[];
}

@Component({
  selector: 'app-tournament-info',
  standalone: true,
  imports: [CommonModule, RouterModule, NgIconComponent, PhasesManager, DynamicForm],
  viewProviders: [provideIcons({ heroArrowLeft, heroPencil, heroTrophy, heroCalendar, heroMapPin, heroArrowRightOnRectangle, heroCheckBadge, heroGlobeAlt })],
  templateUrl: './tournament-info.html',
})
export class TournamentInfo implements OnInit {
  private route = inject(ActivatedRoute);
  private tournamentService = inject(TournamentService);
  private catalogService = inject(CatalogService);
  private phaseService = inject(PhaseService);
  private cdr = inject(ChangeDetectorRef);
  tournamentId: number = 0;

  tournament?: TournamentUI;
  tournamentStates: Catalog[] = [];
  generalStates: Catalog[] = [];

  isLoading = true;

  showCreateModal = false;
  isEditing = false;

  modalConfig = {
    isOpen: false,
    type: 'status' as 'status' | 'confirmation',
    succeeded: false,
    title: '',
    message: '',
    icon: '',
    confirmAction: null as Function | null
  };

  tournamentFields: FormFieldConfig[] = [];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.tournamentId = Number(id);
      this.loadTournamentDetails(this.tournamentId);
      this.loadTournamentStates();
      this.loadGeneralStates();
    }
  }

  loadTournamentDetails(id: number) {
    this.isLoading = true;

    this.tournamentService.getTournamentById(id).subscribe({
      next: (res) => {
        if (res.succeeded && res.data) {
          const data = Array.isArray(res.data) ? res.data[0] : res.data;
          this.tournament = { ...data, isExpanded: false, phases: [] };

          this.phaseService.getPhasesByTournamentId(id).subscribe({
            next: (phaseRes) => {
              if (phaseRes.data && this.tournament) {
                this.tournament.phases = phaseRes.data;
              }
              this.isLoading = false;
              this.cdr.detectChanges();
            },
            error: () => {
              this.isLoading = false;
              this.cdr.detectChanges();
            }
          });
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadTournamentStates() {
    this.catalogService.getTournamentStatus().subscribe({
      next: (res) => {
        if (res.data) {
          this.tournamentStates = res.data;
        }

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.cdr.detectChanges();
      }
    })
  }

  loadGeneralStates() {
    this.catalogService.getGeneralStatus().subscribe({
      next: (res) => {
        if (res.data) {
          this.generalStates = res.data;
        }
      },
      error: (err) => {
        console.error(err);
        this.cdr.detectChanges();
      }
    })
  }

  getStateClass(name: string | null | undefined): string {
    if (!name) return 'bg-slate-100 text-slate-500 border-slate-200';

    const classes: any = {
      'Borrador': 'bg-slate-100 text-slate-600 border-slate-200',
      'DRFT': 'bg-slate-100 text-slate-600 border-slate-200',
      'En Curso': 'bg-orange-100 text-orange-600 border-orange-200',
      'LIVE': 'bg-orange-100 text-orange-600 border-orange-200',
      'Inscripciones Abiertas': 'bg-blue-100 text-blue-600 border-blue-200',
      'OPEN': 'bg-blue-100 text-blue-600 border-blue-200',
      'Finalizado': 'bg-green-100 text-green-600 border-green-200',
      'FINI': 'bg-green-100 text-green-600 border-green-200'
    };
    return classes[name] || 'bg-slate-50 text-slate-400';
  }

  openEditModal(tournament: TournamentResponseDto) {
    this.isEditing = true;

    const tournamentStates = this.tournamentStates.length > 0 ? this.tournamentStates : [];
    const states = this.generalStates.length > 0 ? this.generalStates : [];

    const tournamentStateOptions = tournamentStates.map(tournamentState => ({
      label: tournamentState.name,
      value: tournamentState.id
    }));

    const stateOptions = states.map(state => ({
      label: state.name,
      value: state.id
    }))

    this.tournamentFields = [
      {
        name: 'name', label: 'Nombre del Torneo',
        type: 'text', icon: 'game',
        placeholder: 'Ej: Copa Invierno',
        validators: [Validators.required],
        value: tournament.name
      } as any,
      {
        name: 'description', label: 'Descripción',
        type: 'text', icon: 'game',
        placeholder: 'Detalles del torneo...',
        validators: [Validators.required],
        value: tournament.description
      } as any,
      {
        name: 'startDate', label: 'Fecha de Inicio',
        type: 'date', icon: 'calendar',
        validators: [Validators.required],
        value: this.formatDate(tournament.startDate)
      } as any,
      {
        name: 'endDate', label: 'Fecha de Fin',
        type: 'date', icon: 'calendar',
        validators: [Validators.required],
        value: this.formatDate(tournament.endDate)
      } as any,
      {
        name: 'tournamentStateId',
        label: 'Estado del Torneo',
        type: 'select',
        icon: 'state',
        validators: [Validators.required],
        value: tournament.tournamentStateId,
        options: tournamentStateOptions
      } as any,
      {
        name: 'generalStateId',
        label: 'Estado',
        type: 'select',
        icon: 'state',
        validators: [Validators.required],
        value: tournament.stateId,
        options: stateOptions
      } as any,
    ];

    this.showCreateModal = true;
    this.cdr.detectChanges();
  }

  markAsPublic() {
    if (!this.tournament) return;

    this.isLoading = true;
    this.tournamentService.setPublicTournament(this.tournamentId).subscribe({
      next: (res) => {
        if (res.succeeded) {
          this.loadTournamentDetails(this.tournamentId);

          this.modalConfig = {
            isOpen: true, type: 'status', succeeded: true,
            title: 'Torneo Publicado',
            message: 'Este torneo ahora es el visible en la página principal.',
            icon: 'heroCheckBadge', confirmAction: null
          };
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.showErrorModal('Error al publicar', err);
      }
    });
  }

  closeModal() {
    this.showCreateModal = false;
  }

  private formatDate(date: any): string {
    if (!date) return '';
    return new Date(date).toISOString().split('T')[0];
  }

  handleFormSubmit(formData: any) {

    if (this.isEditing) {
      const dto: UpdateTournamentDto = {
        id: this.tournamentId,
        name: formData.name,
        description: formData.description,
        startDate: formData.startDate,
        endDate: formData.endDate,
        tournamentStateId: formData.tournamentStateId,
        stateId: formData.generalStateId,
      };

      this.isLoading = true;
      this.tournamentService.updateTournament(dto).subscribe({
        next: () => { this.closeModal(); this.loadTournamentDetails(this.tournamentId); },
        error: (err) => { console.error(err); this.isLoading = false; this.cdr.detectChanges(); }
      })
    }
  }

  private showErrorModal(title: string, error?: any) {
    this.isLoading = false;

    let errorMessage = 'Ha ocurrido un error inesperado.';

    if (typeof error === 'string') {
      errorMessage = error;
    } else if (error?.error?.message) {
      errorMessage = error.error.message;
    } else if (error?.message) {
      errorMessage = error.message;
    }

    this.modalConfig = {
      isOpen: true,
      type: 'status',
      succeeded: false,
      title: title,
      message: errorMessage,
      icon: 'heroXMark',
      confirmAction: null
    };

    this.cdr.detectChanges();
  }

  getSequenceOptions(): number[] {
    const total = this.tournament?.phases?.length || 0;
    const options = Array.from({ length: total }, (_, i) => i + 1);
    return [-1, ...options];
  }
}