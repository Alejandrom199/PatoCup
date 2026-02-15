import { Component, Input, inject, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Validators } from '@angular/forms'; // Solo necesitamos Validators
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroPlus, heroPencil, heroTrash, heroChevronUp, heroArrowRightOnRectangle, heroTrophy } from '@ng-icons/heroicons/outline';

// Importamos el Dynamic Form y su Configuración

import { MatchManager } from '../match-manager/match-manager';
import { PhaseResponseDto, CreatePhaseDto, UpdatePhaseDto } from '../../../../../data/features/tournaments/dtos/competition/phase.dto';
import { DynamicForm } from '../../../../components/dynamic-form/dynamic-form';
import { PhaseService } from '../../../../../core/services/competition/phase.service';
import { FormFieldConfig } from '../../../../../core/models/form-config';
import { StatusModal } from "../../../../components/status-modal/status-modal";
import { CatalogService } from '../../../../../core/services/common/catalog.service';
import { Catalog } from '../../../../../core/models/catalog';

interface PhaseUI extends PhaseResponseDto {
  isExpanded?: boolean;
  matches?: any[];
  isFinal?: boolean;
}

@Component({
  selector: 'app-phases-manager',
  standalone: true,
  imports: [CommonModule, NgIconComponent, MatchManager, DynamicForm, StatusModal],
  viewProviders: [provideIcons({ heroTrophy,heroPlus, heroPencil, heroArrowRightOnRectangle, heroTrash, heroChevronUp })],
  templateUrl: './phases-manager.html',
})
export class PhasesManager implements OnInit {

  private _tournamentId: number = 0;

  @Input() set tournamentId(value: number) {
    this._tournamentId = value;
    if (value > 0) this.loadPhases();
  }

  get tournamentId(): number { return this._tournamentId; }

  private phaseService = inject(PhaseService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  phases: PhaseUI[] = [];
  phaseStates: Catalog[] = [];
  generalStates: Catalog[] = [];

  isLoading = false;

  showCreateModal = false;
  isEditing = false;
  selectedPhaseId: number = 0;

  phaseFields: FormFieldConfig[] = [];

  modalConfig = {
    isOpen: false,
    type: 'status' as 'status' | 'confirmation',
    succeeded: false,
    title: '',
    message: '',
    icon: '',
    confirmAction: null as Function | null
  };

  ngOnInit(): void {
    this.loadPhaseStates();
    this.loadGeneralStates();
  }

  loadPhases() {
    this.isLoading = true;
    this.phaseService.getPhasesByTournamentId(this.tournamentId).subscribe({
      next: (res) => {
        if (res.succeeded && res.data) {
          this.phases = res.data.map(phase => ({ ...phase, isExpanded: false }));
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.isLoading = false; this.cdr.detectChanges(); }
    });
  }

  loadPhaseStates() {
    this.catalogService.getPhaseStatus().subscribe({
      next: (res) => {
        if (res.data) {
          this.phaseStates = res.data;
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

  toggleExpand(phase: PhaseUI) {
    phase.isExpanded = !phase.isExpanded;
  }

  getStateClass(name: string): string {
    const classes: any = {
      'Activo': 'bg-green-50 text-green-700 border-green-200',
      'Inactivo': 'bg-gray-100 text-gray-500 border-gray-200',
      'Suspendido': 'bg-red-50 text-red-700 border-red-200',

      'Programada': 'bg-blue-50 text-blue-700 border-blue-200',
      'En Juego': 'bg-orange-50 text-orange-700 border-orange-200',
      'Completada': 'bg-emerald-50 text-emerald-700 border-emerald-200',
    };
    return classes[name] || 'bg-gray-50 text-gray-500 border-gray-100';
  }

  openCreateModal() {
    this.isEditing = false;
    this.selectedPhaseId = 0;
    this.phaseFields = [{
      name: 'name', label: 'Nombre de la Fase', type: 'text',
      placeholder: 'Ej: Octavos de Final', icon: 'game',
      validators: [Validators.required, Validators.minLength(3)]
    }];
    this.showCreateModal = true;
  }

  openEditModal(phase: PhaseResponseDto) {
    this.isEditing = true;
    this.selectedPhaseId = phase.id;

    const phaseStates = this.phaseStates.length > 0 ? this.phaseStates : [];
    const states = this.generalStates.length > 0 ? this.generalStates : [];

    const phaseStateOptions = phaseStates.map(tournamentState => ({
      label: tournamentState.name,
      value: tournamentState.id
    }));

    const stateOptions = states.map(state => ({
      label: state.name,
      value: state.id
    }))

    this.phaseFields = [
      {
        name: 'name',
        type: 'text',
        placeholder: 'Ej: Fase de Grupos...',
        validators: [Validators.required, Validators.minLength(3)],
        icon: 'game',
        value: phase.name
      } as any,
      {
        name: 'sequence',
        label: 'Orden en el Bracket',
        type: 'select',
        icon: 'state',
        validators: [Validators.required],
        value: phase.sequence,
        options: this.getSequenceOptions()
      } as any,
      {
        name: 'phaseStateId',
        label: 'Estado de la Fase',
        type: 'select',
        icon: 'state',
        validators: [Validators.required],
        value: phase.phaseStateId,
        options: phaseStateOptions
      } as any,
      {
        name: 'generalStateId',
        label: 'Estado',
        type: 'select',
        icon: 'state',
        validators: [Validators.required],
        value: phase.stateId,
        options: stateOptions
      } as any,
    ];

    this.showCreateModal = true;
  }

  handleFormSubmit(formData: any) {

    if (this.isEditing) {
      const dto: UpdatePhaseDto = {
        id: this.selectedPhaseId,
        name: formData.name,
        phaseStateId: formData.phaseStateId,
        stateId: formData.generalStateId,
        sequence: formData.sequence
      };

      this.isLoading = true;
      this.phaseService.updatePhase(dto.id, dto).subscribe({
        next: () => { this.closeModal(); this.loadPhases(); },
        error: (err) => { console.error(err); this.isLoading = false; this.cdr.detectChanges(); }
      });

    }
    else {
      const dto: CreatePhaseDto = {
        tournamentId: this.tournamentId,
        name: formData.name
      };

      this.isLoading = true;
      this.phaseService.createPhase(dto).subscribe({
        next: () => { this.closeModal(); this.loadPhases(); },
        error: (err) => { console.error(err); this.isLoading = false; this.cdr.detectChanges(); }
      });
    }
  }

  askDeletePhase(phase: PhaseResponseDto) {
    this.modalConfig = {
      isOpen: true,
      type: 'confirmation',
      succeeded: false,
      title: '¿Eliminar Fase?',
      message: `Se eliminará la fase "${phase.name}" y todas sus partidas. ¿Estás seguro?`,
      icon: 'heroTrash',
      confirmAction: () => this.executeDelete(phase.id)
    };
  }

  executeDelete(id: number) {
    this.closeStatusModal();
    this.isLoading = true;
    this.phaseService.deletePhase(id).subscribe({
      next: () => {
        this.loadPhases();
      },
      error: () => { this.isLoading = false; this.cdr.detectChanges(); }
    });
  }

  private getSequenceOptions(): any[] {
    const total = this.phases.length;
    const options = Array.from({ length: total }, (_, i) => ({
      label: `Posición #${i + 1}`,
      value: i + 1
    }));
    return [{ label: 'Ocultar', value: -1 }, ...options];
  }


  setAsFinal(phase: PhaseUI) {
    this.isLoading = true;

    this.phaseService.setFinalPhase(this.tournamentId, phase.id).subscribe({
      next: (res) => {
        if (res.succeeded) {
          this.loadPhases();
          // Opcional: puedes mostrar un modal de éxito aquí también
        }
      },
      error: (err) => {
        this.isLoading = false;
        
        const errorMsg = err.error?.message || "Ocurrió un error al intentar establecer la final.";

        this.modalConfig = {
          isOpen: true,
          type: 'status',
          succeeded: false,
          title: 'Validación de Fase',
          message: errorMsg,
          icon: 'heroTrophy',
          confirmAction: null
        };

        this.cdr.detectChanges();
      }
    });
  }

  closeModal() { this.showCreateModal = false; }
  closeStatusModal() { this.modalConfig.isOpen = false; }
  onModalConfirm() { if (this.modalConfig.confirmAction) this.modalConfig.confirmAction(); }
}