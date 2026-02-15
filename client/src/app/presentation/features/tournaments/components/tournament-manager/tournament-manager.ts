import { CommonModule } from "@angular/common";
import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { NgIconComponent, provideIcons } from "@ng-icons/core";
import {
  heroSquares2x2,
  heroUserCircle,
  heroCalendar,
  heroTrophy,
  heroFunnel,
  heroExclamationCircle,
  heroTrash,
  heroChevronUp,
  heroArrowRightOnRectangle
} from '@ng-icons/heroicons/outline';
import { DynamicForm } from "../../../../components/dynamic-form/dynamic-form";
import { FormsModule, Validators } from "@angular/forms";
import { Catalog } from '../../../../../core/models/catalog';
import { StatusModal } from "../../../../components/status-modal/status-modal";
import { Pagination } from "../../../../components/pagination/pagination";
import { FormFieldConfig } from "../../../../../core/models/form-config";
import { Router } from "@angular/router";
import { CatalogService } from "../../../../../core/services/common/catalog.service";
import { TournamentService } from "../../../../../core/services/competition/tournament.service";
import { TournamentQuery, TournamentResponseDto } from "../../../../../data/features/tournaments/dtos/competition/tournament.dto";
import { StatusService } from "../../../../../core/services/status/status.service";

interface TournamentUI extends TournamentResponseDto {
    isExpanded?: boolean;
}

@Component({
  selector: 'app-tournament-manager',
  standalone: true,
  imports: [CommonModule, NgIconComponent, DynamicForm, FormsModule, StatusModal, Pagination],
  viewProviders: [provideIcons({
    heroSquares2x2,
    heroUserCircle,
    heroArrowRightOnRectangle,
    heroCalendar,
    heroTrophy,
    heroFunnel,
    heroExclamationCircle,
    heroTrash,
    heroChevronUp
  })],
  templateUrl: './tournament-manager.html',
})
export class TournamentManager implements OnInit {
    @ViewChild('fName') fName!: ElementRef;
    @ViewChild('fDesc') fDesc!: ElementRef;
    @ViewChild('fStatus') fStatus!: ElementRef;
    @ViewChild('fStart') fStart!: ElementRef;
    @ViewChild('fEnd') fEnd!: ElementRef;

    tournaments: TournamentUI[] = [];
    tournamentStatuses: Catalog[] = [];
    totalTournaments = 0;
    liveCount = 0;
    draftCount = 0;
    showCreateModal = false;
    currentFilter: TournamentQuery = { pageNumber: 1, pageSize: 10 };
    totalItemsFromBackend = 0;

    modalConfig = {
        isOpen: false,
        type: 'status' as 'status' | 'confirmation',
        succeeded: false,
        title: '',
        message: '',
        icon: '',
        confirmAction: null as Function | null
    };

    tournamentFields: FormFieldConfig[] = [
        {
            name: 'name', label: 'Nombre del Torneo',
            type: 'text', icon: 'game',
            placeholder: 'Ej: Copa Invierno',
            validators: [Validators.required]
        },
        {
            name: 'description', label: 'Descripción',
            type: 'text', icon: 'game',
            placeholder: 'Detalles del torneo...',
            validators: [Validators.required]
        },
        {
            name: 'startDate', label: 'Fecha de Inicio',
            type: 'date', icon: 'calendar',
            validators: [Validators.required]
        },
        {
            name: 'endDate', label: 'Fecha de Fin',
            type: 'date', icon: 'calendar',
            validators: [Validators.required]
        },
    ];

    constructor(
        private tournamentsService: TournamentService,
        private catalogService: CatalogService,
        private statusService: StatusService,
        private router: Router,
        private cdr: ChangeDetectorRef
    ) { }

    ngOnInit() {
        this.loadTournamentStatuses();
        this.loadTournaments();
    }

    loadTournamentStatuses() {
        this.catalogService.getTournamentStatus().subscribe({
            next: (res: any) => {
                this.tournamentStatuses = Array.isArray(res) ? res : res.data;
                this.cdr.detectChanges();
            }
        });
    }

    loadTournaments() {
        this.tournamentsService.getAllTournaments(this.currentFilter).subscribe({
            next: (response) => {
                if (response.succeeded && response.data) {
                    this.tournaments = response.data;

                    this.totalItemsFromBackend = 100;

                    this.updateDashboardCounters();
                    this.cdr.detectChanges();
                }
            }
        });
    }

    onPageChange(page: number) {
        this.currentFilter.pageNumber = page;
        this.loadTournaments();
    }

    onPageSizeChange(size: number) {
        this.currentFilter.pageSize = size;
        this.currentFilter.pageNumber = 1;
        this.loadTournaments();
    }

    applyFilters() {
        this.currentFilter = {
            ...this.currentFilter,
            name: this.fName.nativeElement.value.trim() || undefined,
            description: this.fDesc.nativeElement.value.trim() || undefined,
            tournamentStateId: this.fStatus.nativeElement.value ? Number(this.fStatus.nativeElement.value) : undefined,
            startDate: this.fStart.nativeElement.value || undefined,
            endDate: this.fEnd.nativeElement.value || undefined,
            pageNumber: 1
        };
        this.loadTournaments();
    }

    updateDashboardCounters() {
        this.liveCount = this.tournaments.filter(t => t.tournamentStateName === 'En Curso' || t.tournamentStateName === 'LIVE').length;
        this.draftCount = this.tournaments.filter(t => t.tournamentStateName === 'Borrador' || t.tournamentStateName === 'DRFT').length;
    }

    getStateClass(name: string): string {
        const classes: any = {
            'Borrador': 'bg-slate-50 text-slate-600 border-slate-200',
            'En Curso': 'bg-orange-50 text-orange-700 border-orange-100',
            'Inscripciones Abiertas': 'bg-blue-50 text-blue-700 border-blue-100',
            'Finalizado': 'bg-emerald-50 text-emerald-700 border-emerald-100',

            'Activo': 'bg-green-50 text-green-700 border-green-200',
            'Inactivo': 'bg-gray-100 text-gray-500 border-gray-200',
            'Suspendido': 'bg-red-50 text-red-700 border-red-200'
        };
        return classes[name] || 'bg-gray-50 text-gray-500';
    }

    openCreateModal() {
        this.showCreateModal = true;
        this.cdr.detectChanges();
    }

    goToInfo(id: number) {
        this.router.navigate(['competition/tournaments', id, 'details']);
    }

    deleteTournament(id: number) {
        if (confirm('¿Estás seguro de que quieres eliminar este torneo? Esta acción no se puede deshacer.')) {

            this.tournamentsService.deleteTournament(id).subscribe({
                next: (res) => {
                    if (res.succeeded) {
                        this.statusService.show(true, 'Torneo eliminado correctamente');
                        this.loadTournaments();
                    } else {
                        this.statusService.show(false, res.message || 'No se pudo eliminar');
                    }
                },
                error: (err) => {
                    this.statusService.show(false, 'Error al intentar eliminar');
                    console.error(err);
                }
            });
        }
    }

    handleCreateTournament(data: any) {
        this.tournamentsService.createTournament({ ...data, statusId: 1 }).subscribe({
            next: (res) => {
                if (res.succeeded) {
                    this.showCreateModal = false;

                    this.statusService.show(true, '¡Torneo creado con éxito!');

                    this.router.navigate(['competition/tournaments', res.data, 'details']);
                }
            },
            error: (err) => {
                const errorMsg = err.error?.message || 'No se pudo crear el torneo';
                this.statusService.show(false, errorMsg);
                this.cdr.detectChanges();
            }
        });
    }

    askDeleteTournament(tournament: TournamentResponseDto) {
        this.modalConfig = {
            isOpen: true,
            type: 'confirmation',
            succeeded: false,
            title: '¿Eliminar Torneo?',
            message: `Al eliminar el torneo "${tournament.name}", también se eliminarán todas sus fases.
            Esta acción no se puede deshacer. ¿Deseas continuar?`,

            icon: 'heroTrash',
            confirmAction: () => this.executeDelete(tournament.id)
        };
    }

    executeDelete(id: number) {
        this.closeModal();

        this.tournamentsService.deleteTournament(id).subscribe({
            next: (res) => {
                if (res.succeeded) {
                    this.modalConfig = {
                        isOpen: true,
                        type: 'status',
                        succeeded: true,
                        title: '¡Eliminado!',
                        message: 'El torneo ha sido eliminado correctamente.',
                        icon: '',
                        confirmAction: null
                    };
                    this.loadTournaments();
                } else {
                    this.showErrorModal(res.message);
                }
            },
            error: () => this.showErrorModal('Ocurrió un error al eliminar.')
        });
    }

    showErrorModal(msg: string) {
        this.modalConfig = {
            isOpen: true,
            type: 'status',
            succeeded: false,
            title: 'Error',
            message: msg,
            icon: '',
            confirmAction: null
        };
    }

    closeModal() {
        this.modalConfig.isOpen = false;
    }

    onModalConfirm() {
        if (this.modalConfig.confirmAction) {
            this.modalConfig.confirmAction();
        }
    }
}