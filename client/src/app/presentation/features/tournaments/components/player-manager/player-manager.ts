import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconComponent, provideIcons } from "@ng-icons/core";
import { Validators } from '@angular/forms';
import {
  heroTrash, heroUsers, heroClock, heroCheckBadge,
  heroCheckCircle, heroXCircle, heroEye, heroEyeSlash,
  heroComputerDesktop, heroPencilSquare, heroFunnel,
  heroArrowRightOnRectangle,
  heroChevronUp,
  heroListBullet
} from '@ng-icons/heroicons/outline';

import { PlayerResponseDto, UpdatePlayerDto } from '../../../../../data/features/tournaments/dtos/competition/player.dto';
import { PlayerService } from '../../../../../core/services/competition/player.service';
import { CatalogService } from '../../../../../core/services/common/catalog.service';
import { FormFieldConfig } from '../../../../../core/models/form-config';
import { PlayerStates, ViewMode } from '../../../../../core/constants/player-status.enums';
import { Catalog } from '../../../../../core/models/catalog';
import { DynamicForm } from '../../../../components/dynamic-form/dynamic-form';
import { StatusModal } from '../../../../components/status-modal/status-modal';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-player-manager',
  standalone: true,
  templateUrl: './player-manager.html',
  imports: [CommonModule, NgIconComponent, DynamicForm, StatusModal],
  viewProviders: [provideIcons({
    heroTrash, heroUsers, heroClock, heroCheckBadge,
    heroCheckCircle, heroXCircle, heroEye, heroEyeSlash,
    heroComputerDesktop, heroPencilSquare, heroFunnel,
    heroArrowRightOnRectangle, heroChevronUp, select: heroListBullet
  })],
})
export class PlayerManager implements OnInit, OnDestroy {
  private refreshInterval: any;

  private titleService = inject(Title);
  private playerService = inject(PlayerService);
  private catalogService = inject(CatalogService);
  private cdr = inject(ChangeDetectorRef);

  public readonly ViewMode = ViewMode;

  players: PlayerResponseDto[] = [];
  filteredPlayers: PlayerResponseDto[] = [];
  generalStates: Catalog[] = [];

  viewMode: ViewMode.Active | ViewMode.Pending = ViewMode.Active;
  searchTerm: string = '';

  totalPlayers = 0;
  pendingCount = 0;
  activeCount = 0;
  isLoading = false;
  isEditing = false;

  showEditModal = false;
  selectedPlayerId: number = 0;
  playerFields: FormFieldConfig[] = [];
  visibleIps = new Set<number>();

  modalConfig = {
    isOpen: false,
    type: 'confirmation' as 'status' | 'confirmation',
    title: '',
    message: '',
    icon: '',
    actionType: ''
  };

  ngOnInit() {
    this.titleService.setTitle('PatoCup - Jugadores');
    this.loadPlayers();
    this.loadGeneralStates();
    this.startAutoRefresh();
  }

  loadPlayers() {
    this.isLoading = true;
    this.playerService.getAllPlayers(1, 100).subscribe({
      next: (res) => {
        if (res.succeeded && res.data) {
          this.players = res.data;
          this.updateStats();
          this.applyViewFilter();
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => this.isLoading = false
    });
  }

  loadGeneralStates() {
    this.catalogService.getGeneralStatus().subscribe(res => {
      if (res.data) this.generalStates = res.data;
    });
  }

  updateStats() {
    this.totalPlayers = this.players.length;
    this.pendingCount = this.players.filter(p => p.playerStateName?.toLowerCase() === PlayerStates.Pendiente).length;
    this.activeCount = this.players.filter(p =>
      p.playerStateName?.toLowerCase() === PlayerStates.Aceptado ||
      p.playerStateName?.toLowerCase() === PlayerStates.Aprobado
    ).length;
  }

  applyViewFilter() {
    let temp = this.players.filter(p => {
      const status = p.playerStateName?.toLowerCase();
      return this.viewMode === ViewMode.Active
        ? (status === PlayerStates.Aceptado || status === PlayerStates.Aprobado)
        : (status === PlayerStates.Pendiente);
    });

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      temp = temp.filter(p =>
        p.nickname.toLowerCase().includes(term) ||
        (p.gameId && p.gameId.toLowerCase().includes(term))
      );
    }
    this.filteredPlayers = temp;
  }

  setMode(mode: ViewMode.Active | ViewMode.Pending) {
    this.viewMode = mode;
    this.applyViewFilter();
  }

  onProcessRequest(player: PlayerResponseDto, accept: boolean) {
    const statusId = accept ? 2 : 3; // Lógica de tu backend
    this.playerService.processRequest(player.id, statusId).subscribe(res => {
      if (res.succeeded) this.loadPlayers();
    });
  }

  openEditModal(player: PlayerResponseDto) {
    this.isEditing = true;
    this.selectedPlayerId = player.id;

    const stateOptions = this.generalStates.map(state => ({
      label: state.name ?? 'Sin Nombre',
      value: state.id
    }));

    this.playerFields = [
      {
        name: 'nickname',
        label: 'Nickname',
        type: 'text',
        icon: 'user',
        value: player.nickname,
        validators: [Validators.required]
      },
      {
        name: 'gameId',
        label: 'Game ID',
        type: 'text',
        icon: 'game',
        value: player.gameId
      },
      {
        name: 'stateId',
        label: 'Estado del Jugador',
        type: 'select',
        icon: 'select',
        value: player.stateId,
        options: stateOptions
      }
    ];

    this.showEditModal = true;
  }

  handleFormSubmit(formData: any) {
    const dto: UpdatePlayerDto = {
      id: this.selectedPlayerId,
      nickname: formData.nickname,
      gameId: formData.gameId,
      stateId: formData.stateId
    };

    this.isLoading = true;
    this.playerService.updatePlayer(dto.id, dto).subscribe({
      next: () => {
        this.showEditModal = false;
        this.loadPlayers();
      },
      error: () => this.isLoading = false
    });
  }

  askDelete(player: PlayerResponseDto) {
    this.selectedPlayerId = player.id;
    this.modalConfig = {
      isOpen: true,
      type: 'confirmation',
      title: 'Eliminar Jugador',
      message: `¿Estás seguro de eliminar a "${player.nickname}"? Esta acción no se puede deshacer.`,
      icon: 'heroTrash',
      actionType: 'delete'
    };
  }

  onModalConfirm() {
    if (this.modalConfig.actionType === 'delete' && this.selectedPlayerId) {
      this.playerService.deletePlayer(this.selectedPlayerId).subscribe(() => {
        this.closeStatusModal();
        this.loadPlayers();
      });
    }
  }

  toggleIpVisibility(playerId: number) {
    this.visibleIps.has(playerId) ? this.visibleIps.delete(playerId) : this.visibleIps.add(playerId);
  }

  closeStatusModal() { this.modalConfig.isOpen = false; this.selectedPlayerId = 0; }

  getStateClass(name: string): string {
    const classes: any = {
      'Activo': 'bg-green-50 text-green-700 border-green-200',
      'Inactivo': 'bg-gray-100 text-gray-500 border-gray-200',
      'Pendiente': 'bg-orange-50 text-orange-600 border-orange-200',
    };
    return classes[name] || 'bg-gray-50 text-gray-500 border-gray-100';
  }

  startAutoRefresh() {
    const seconds = 3;

    this.refreshInterval = setInterval(() => {
      if (!this.showEditModal && !this.modalConfig.isOpen && !this.isLoading) {
        console.log('Refrescando lista de jugadores automáticamente...');
        this.loadPlayers();
      }
    }, (seconds * 1000));
  }

  ngOnDestroy() {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }
}