import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { NgIcon, provideIcons } from "@ng-icons/core";
import { PlayerResponseDto, UpdatePlayerDto } from '../../../../../data/features/tournaments/dtos/competition/player.dto';
import { PlayerService } from '../../../../../core/services/competition/player.service';
import { FormFieldConfig } from '../../../../../core/models/form-config';
import {
  heroArrowRightOnRectangle,
  heroSquares2x2,
  heroUserCircle,
  heroCalendar,
  heroTrophy,
  heroFunnel,
  heroExclamationCircle,
  heroTrash,
  heroUsers,
  heroClock,
  heroCheckBadge,
  heroCheckCircle,
  heroXCircle,
  heroEye,
  heroEyeSlash,
  heroExclamationTriangle,
  heroComputerDesktop,
  heroPencilSquare
} from '@ng-icons/heroicons/outline';
import { PlayerStatus, ViewMode } from '../../../../../core/constants/player-status.enums';

@Component({
  selector: 'app-player-manager',
  templateUrl: './player-manager.html',
  imports: [NgIcon],
    viewProviders: [provideIcons({
    heroArrowRightOnRectangle,
    heroCalendar,
    heroTrophy,
    heroFunnel,
    heroExclamationCircle,
    heroTrash,
    heroUsers, 
    heroClock, 
    heroCheckBadge, 
    heroCheckCircle, 
    heroXCircle,
    heroEye, 
    heroEyeSlash, 
    heroExclamationTriangle,
    heroComputerDesktop,
    heroPencilSquare
  })],
})
export class PlayerManager implements OnInit {
  private playerService = inject(PlayerService);
  private cdr = inject(ChangeDetectorRef);
  
  public readonly ViewMode = ViewMode;

  players: PlayerResponseDto[] = [];
  filteredPlayers: PlayerResponseDto[] = [];
  
  // activo para comunidad aceptada, ViewMode.Pending para solicitudes
  viewMode: ViewMode.Active | ViewMode.Pending = ViewMode.Active;
  
  // Stats para las cards superiores
  totalPlayers = 0;
  pendingCount = 0;
  activeCount = 0;

  showEditModal = false;
  selectedPlayer: PlayerResponseDto | null = null;

  visibleIps = new Set<number>();
  
  // Definición de campos para el DynamicForm
  playerFields: FormFieldConfig[] = [
    { 
      name: 'nickname', 
      label: 'Nickname', 
      type: 'text', 
      icon: 'user', 
      placeholder: 'Ej: PatoGamer',
      value: ''
    },
    { 
      name: 'gameId', 
      label: 'Game ID', 
      type: 'text', 
      icon: 'game', 
      placeholder: 'ID del juego...',
      value: '' 
    },
    { 
      name: 'stateId', 
      label: 'Estado del Jugador', 
      type: 'select', 
      icon: 'select',
      value: 1,
      options: [
        { label: 'Activo', value: 1 },
        { label: 'Inactivo', value: 0 }
      ] 
    }
  ];

  // Configuración para StatusModal (Borrados y Alertas)
  modalConfig = {
    isOpen: false,
    type: 'confirmation' as 'status' | 'confirmation',
    title: '',
    message: '',
    icon: '',
    confirmColor: 'red' as 'red' | 'blue',
    actionType: '' // Para saber si estamos confirmando un delete u otra cosa
  };

  ngOnInit() {
    this.loadPlayers();
  }

  loadPlayers() {
  this.playerService.getAllPlayers(1, 100).subscribe({
    next: (res) => {
      if (res.succeeded && res.data) {
        this.players = res.data;
        this.updateStats();
        this.applyViewFilter();
        this.cdr.detectChanges();
      }
    },
    error: (err) => console.error("Error cargando jugadores", err)
  });
}

  updateStats() {
    this.totalPlayers = this.players.length;
    this.pendingCount = this.players.filter(p => p.playerStateName === PlayerStatus.Pendiente).length;
    this.activeCount = this.players.filter(p => p.playerStateName === PlayerStatus.Aceptado).length;
  }

  applyViewFilter() {
    if (!this.players || this.players.length === 0) {
      this.filteredPlayers = [];
      return;
    }

    this.filteredPlayers = [...this.players.filter(p => {
      const status = p.playerStateName?.toLowerCase();
      if (this.viewMode === ViewMode.Active) {
        return status === PlayerStatus.Aceptado || status === PlayerStatus.Aprobado;
      } else {
        return status === PlayerStatus.Pendiente;
      }
    })];
  }

  setMode(mode: ViewMode.Active | ViewMode.Pending) {
  this.viewMode = mode;
  this.applyViewFilter();
  
  this.cdr.detectChanges(); 
}

  // Lógica para procesar (Aceptar/Rechazar)
  onProcessRequest(player: PlayerResponseDto, accept: boolean) {
    const statusId = accept ? 2 : 3; // 2: Aceptado, 3: Rechazado según tu lógica de C#
    this.playerService.processRequest(player.id, statusId).subscribe(res => {
      if (res.succeeded) this.loadPlayers();
    });
  }

  openEditModal(player: PlayerResponseDto) {
    this.selectedPlayer = player;
    // Pre-cargamos los valores en los campos del formulario
    this.playerFields[0].value = player.nickname;
    this.playerFields[1].value = player.gameId;
    this.playerFields[2].value = player.stateId;
    this.showEditModal = true;
  }

  handleUpdatePlayer(formData: any) {
    if (!this.selectedPlayer) return;

    const dto: UpdatePlayerDto = {
      id: this.selectedPlayer.id,
      nickname: formData.nickname,
      gameId: formData.gameId,
      stateId: formData.stateId
    };

    this.playerService.updatePlayer(dto.id, dto).subscribe(() => {
      this.showEditModal = false;
      this.loadPlayers(); // Recargamos la tabla
    });
  }

  /**
   * Devuelve las clases de color según el estado de la solicitud del jugador
   * @param statusName Nombre del estado (Pendiente, Aceptado, Rechazado)
   */
  getPlayerStatusClass(statusName: string | undefined): string {
    if (!statusName) return 'border-slate-200 bg-slate-50 text-slate-400';

    switch (statusName.toLowerCase()) {
      case PlayerStatus.Pendiente:
        return 'border-orange-200 bg-orange-50 text-orange-600';
      case PlayerStatus.Aceptado:
      case PlayerStatus.Aprobado:
        return 'border-green-200 bg-green-50 text-green-600';
      case 'rechazado':
      case 'denegado':
        return 'border-red-200 bg-red-50 text-red-600';
      default:
        return 'border-slate-200 bg-slate-50 text-slate-500';
    }
  }

  /**
   * Método complementario para el estado lógico del sistema (Activo/Inactivo)
   */
  getStateClass(stateName: string | undefined): string {
    if (!stateName) return 'border-slate-200 bg-slate-50 text-slate-400';
    
    return stateName.toLowerCase() === 'activo' 
      ? 'border-blue-200 bg-blue-50 text-blue-600' 
      : 'border-slate-300 bg-slate-100 text-slate-500';
  }


  askDelete(player: PlayerResponseDto) {
    this.selectedPlayer = player;
    this.modalConfig = {
      isOpen: true,
      type: 'confirmation',
      title: 'Eliminar Jugador',
      message: `¿Estás seguro de eliminar a "${player.nickname}"? Esta acción no se puede deshacer.`,
      icon: 'heroTrash',
      confirmColor: 'red',
      actionType: 'delete'
    };
  }

  onModalConfirm() {
    if (this.modalConfig.actionType === 'delete' && this.selectedPlayer) {
      this.playerService.deletePlayer(this.selectedPlayer.id).subscribe(() => {
        this.closeModal();
        this.loadPlayers();
      });
    }
  }

  /**
   * Alterna la visibilidad de la IP de un jugador específico
   * @param playerId ID del jugador a mostrar/ocultar
   */
  toggleIpVisibility(playerId: number): void {
    if (this.visibleIps.has(playerId)) {
      this.visibleIps.delete(playerId);
    } else {
      this.visibleIps.add(playerId);
    }
  }

  closeModal() {
    this.modalConfig.isOpen = false;
    this.selectedPlayer = null;
  }
}