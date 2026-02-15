import { Component, Input, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Validators } from '@angular/forms';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroPlus, heroTrash, heroTrophy, heroClock, heroArrowRightOnRectangle, heroPencil, heroXMark, heroHashtag, heroNumberedList } from '@ng-icons/heroicons/outline';

import { MatchResponseDto, CreateMatchDto, RegisterResultDto } from '../../../../../data/features/tournaments/dtos/competition/match.dto';
import { DynamicForm } from '../../../../components/dynamic-form/dynamic-form';
import { FormFieldConfig } from '../../../../../core/models/form-config';
import { MatchService } from '../../../../../core/services/competition/match.service';
import { PlayerService } from '../../../../../core/services/competition/player.service';
import { StatusModal } from "../../../../components/status-modal/status-modal";
import { Observable } from 'rxjs';
import { CatalogService } from '../../../../../core/services/common/catalog.service';
import { Catalog } from '../../../../../core/models/catalog';

interface MatchUI extends MatchResponseDto {
  isExpanded?: boolean;
}

@Component({
  selector: 'app-match-manager',
  standalone: true,
  imports: [CommonModule, NgIconComponent, DynamicForm, StatusModal],
  viewProviders: [provideIcons({ 
    heroPlus, heroTrash, heroTrophy, heroClock, 
    heroArrowRightOnRectangle, heroPencil, heroXMark, heroHashtag, heroNumberedList
  })],
  templateUrl: './match-manager.html',
})
export class MatchManager implements OnInit {
  
  private _phaseId: number = 0;

  @Input() set phaseId(value: number){
    this._phaseId = value;
    if (value > 0) this.loadMatches();
  } 
  
  get phaseId(): number { return this._phaseId; }

  playerOptions: { value: any; label: string }[] = [];

  private matchService = inject(MatchService);
  private playerService = inject(PlayerService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  matches: MatchUI[] = [];
  isLoading = false;

  showCreateModal = false;
  matchStates: Catalog[] = [];
  generalStates: Catalog[] = [];

  isEditing = false;
  selectedMatchId: number = 0;

  isRegisteringResult = false;

  matchFields: FormFieldConfig[] = [];

  modalConfig = {
    isOpen: false,
    type: 'status' as 'status' | 'confirmation',
    succeeded: false,
    title: '',
    message: '',
    icon: '',
    confirmAction: null as Function | null
  };

  ngOnInit() {
    if (this.phaseId) {
      this.loadMatches();
      this.loadPlayers();
      this.loadMatchStates();
      this.loadGeneralStates();
    }
  }

  loadPlayers() {
    this.playerService.getPlayersSelect().subscribe({
      next: (res) => {
        if (res.succeeded && res.data) {
          this.playerOptions = res.data.map(p => ({
            value: p.id,
            label: `${p.nickname}`
          }));
        }
      },
      error: (err) => console.error('Error cargando jugadores', err)
    });
  }

  loadMatches() {
    this.isLoading = true;
    this.matchService.getMatchesByPhaseId(this.phaseId).subscribe({
      next: (res) => {
        if (res.succeeded) this.matches = res.data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getStateClass(name: string): string {
    const classes: any = {
      'Borrador': 'bg-slate-50 text-slate-600 border-slate-200',
      'En Curso': 'bg-orange-50 text-orange-700 border-orange-100',
      'Finalizado': 'bg-emerald-50 text-emerald-700 border-emerald-100',
      'ACTIVO': 'bg-green-50 text-green-700 border-green-200',
      'INACTIVO': 'bg-gray-50 text-gray-500 border-gray-200',
      'SUSPENDIDO': 'bg-red-50 text-red-700 border-red-200'
    };
    return classes[name] || 'bg-slate-50 text-slate-500 border-slate-100';
  }

  openCreateModal() {
    this.matchFields = [
      {
        name: 'player1Id',
        label: 'Jugador 1',
        type: 'select',
        placeholder: 'Selecciona al primer jugador',
        validators: [Validators.required],
        icon: 'user',
        options: this.playerOptions 
      },
      {
        name: 'player2Id',
        label: 'Jugador 2',
        type: 'select',
        placeholder: 'Selecciona al rival',
        validators: [Validators.required],
        icon: 'user',
        options: this.playerOptions
      }
    ];

    this.showCreateModal = true;
  }

  loadMatchStates() {
    this.catalogService.getMatchStatus().subscribe({
      next: (res) => {
        if(res.data){
          this.matchStates = res.data;
        }

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.cdr.detectChanges();
      }
    })
  }

  loadGeneralStates(){
    this.catalogService.getGeneralStatus().subscribe({
      next: (res) => {
        if(res.data){
          this.generalStates = res.data;
        }
      },
      error: (err) => {
        console.error(err);
        this.cdr.detectChanges();
      }
    })
  }

  openEditModal(match: MatchResponseDto) {
    this.isEditing = true;
    this.selectedMatchId = match.id;

    const matchStates = this.matchStates.length > 0 ? this.matchStates : [];
    const states = this.generalStates.length > 0 ? this.generalStates : [];

    const matchStateOptions = matchStates.map(matchState => ({
      label: matchState.name,
      value: matchState.id
    }));

    const stateOptions = states.map(state => ({
      label: state.name,
      value: state.id
    }))

    this.matchFields = [
      {
        name: 'player1Id',
        label: 'Jugador 1',
        type: 'select',
        placeholder: 'Selecciona al primer jugador',
        validators: [Validators.required],
        icon: 'user',
        options: this.playerOptions 
      } as any,
      {
        name: 'player2Id',
        label: 'Jugador 2',
        type: 'select',
        placeholder: 'Selecciona al rival',
        validators: [Validators.required],
        icon: 'user',
        options: this.playerOptions
      } as any,
      {
        name: 'matchStateId', 
        label: 'Estado de la Partida',
        type: 'select', 
        icon: 'state',
        validators: [Validators.required],
        value: match.matchStateId,
        options: matchStateOptions
      } as any,
      {
        name: 'generalStateId', 
        label: 'Estado',
        type: 'select', 
        icon: 'state',
        validators: [Validators.required],
        value: match.stateId,
        options: stateOptions
      } as any,
    ];

    this.showCreateModal = true;
  }

  closeModal() {
  this.showCreateModal = false;
  this.isEditing = false;
  this.isRegisteringResult = false;
  this.selectedMatchId = 0;
  this.isLoading = false;
}

  openResultModal(match: MatchResponseDto) {
    this.isEditing = false;
    this.isRegisteringResult = true;
    this.selectedMatchId = match.id;

    this.matchFields = [
      {
        name: 'scorePlayer1',
        label: `Rondas ganadas de ${match.player1Name}`,
        type: 'number',
        value: match.scorePlayer1 || 0,
        validators: [Validators.required, Validators.min(0)],
        icon: 'hashtag',
        gridCols: 'col-span-1'
      } as any,
      {
        name: 'scorePlayer2',
        label: `Rondas ganadas de ${match.player2Name}`,
        type: 'number',
        value: match.scorePlayer2 || 0,
        validators: [Validators.required, Validators.min(0)],
        icon: 'hashtag',
        gridCols: 'col-span-1'
      } as any,
      {
        name: 'winnerId',
        label: 'Ganador de la Partida',
        type: 'select',
        placeholder: 'Selecciona al ganador',
        validators: [Validators.required],
        options: [
          { value: match.player1Id, label: match.player1Name },
          { value: match.player2Id, label: match.player2Name },
        ],
        value: match.winnerId || 0
      } as any
    ];

    this.showCreateModal = true;
  }

  handleFormSubmit(formData: any) {
    this.isLoading = true;

    // Registro de resultados (Marcador y Ganador)
    if (this.isRegisteringResult) {
      const resultDto: RegisterResultDto = {
        id: this.selectedMatchId,
        scorePlayer1: Number(formData.scorePlayer1),
        scorePlayer2: Number(formData.scorePlayer2),
        winnerId: Number(formData.winnerId)
      };

      this.matchService.registerResult(resultDto).subscribe({
        next: (res) => {
          if (res.succeeded) {
            this.closeModal();
            this.loadMatches();
          } 
          else {
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        },
        error: (err) => {
          this.showErrorModal('Error al registrar resultado:', err);
        }
      });
      return;
    }

    if (formData.player1Id == formData.player2Id) {
    this.isLoading = false;
    this.modalConfig = {
      isOpen: true,
      type: 'status',
      succeeded: false,
      title: 'Selección Inválida',
      message: 'Un jugador no puede enfrentarse a sí mismo. Por favor, selecciona rivales diferentes.',
      icon: 'heroXMark',
      confirmAction: null 
    };
    return;
  }

    const dto: any = {
      id: this.selectedMatchId,
      phaseId: this.phaseId,
      player1Id: Number(formData.player1Id),
      player2Id: Number(formData.player2Id),
      matchStateId: formData.matchStateId || 1,
      stateId: formData.generalStateId || 1
    };

    // Crear o Actualizar
    const request: Observable<any> = this.isEditing
      ? this.matchService.updateMatch(dto)
      : this.matchService.createMatch(dto);

    request.subscribe({
      next: (res) => {
        if (res.succeeded) {
          this.closeModal();
          this.loadMatches();
        } 
        else {
          this.isLoading = false;
          this.showErrorModal(res.message || 'No se pudo guardar la partida');
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.showErrorModal('Error de conexión con el servidor');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  deleteMatch(id: number) {
    if (!confirm('¿Eliminar esta partida?')) return;
    this.isLoading = true;
    this.matchService.deleteMatch(id).subscribe({
      next: () => this.loadMatches(),
      error: () => { this.isLoading = false; this.cdr.detectChanges(); }
    });
  }

  askDeletePhase(match: MatchResponseDto) {
    this.modalConfig = {
      isOpen: true,
      type: 'confirmation',
      succeeded: false,
      title: '¿Eliminar Partida?',
      message: `Se eliminará la partida de "${match.player1Name} vs ${match.player2Name}". ¿Estás seguro?`,
      icon: 'heroTrash',
      confirmAction: () => this.executeDelete(match.id)
    };
  }

  executeDelete(id: number) {
    this.closeStatusModal();
    this.isLoading = true;
    this.matchService.deleteMatch(id).subscribe({
      next: () => {
        this.loadMatches();
      },
      error: () => { this.isLoading = false; this.cdr.detectChanges(); }
    });
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

  closeStatusModal() { this.modalConfig.isOpen = false; }
  onModalConfirm() { if (this.modalConfig.confirmAction) this.modalConfig.confirmAction(); }
}