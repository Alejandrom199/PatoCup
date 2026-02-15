import { Component, OnInit, inject, ChangeDetectorRef, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
    heroShieldCheck, heroClock, heroUser, heroComputerDesktop,
    heroListBullet, heroEye, heroEyeSlash, heroFunnel, heroExclamationTriangle,
    heroArrowPath,
    heroInformationCircle
} from '@ng-icons/heroicons/outline';
import { Pagination } from '../../../../components/pagination/pagination';
import { AuditService } from '../../../../../core/services/security/audit.service';
import { AuditLogResponseDto } from '../../../../../data/features/tournaments/dtos/security/audit.dto';
import { StatusModal } from "../../../../components/status-modal/status-modal";


@Component({
    selector: 'app-audit-manager',
    standalone: true,
    imports: [CommonModule, NgIconComponent, Pagination, StatusModal],
    viewProviders: [provideIcons({
        heroShieldCheck, heroClock, heroUser, heroComputerDesktop,
        heroListBullet, heroEye, heroEyeSlash, heroFunnel, heroExclamationTriangle,
        heroArrowPath, heroInformationCircle
    })],
    templateUrl: './audit-manager.html'
})
export class AuditManager implements OnInit {
    @ViewChild('fUser') fUser!: ElementRef;
    @ViewChild('fAction') fAction!: ElementRef;
    @ViewChild('fDate') fDate!: ElementRef;

    private auditService = inject(AuditService);
    private cdr = inject(ChangeDetectorRef);

    logs: AuditLogResponseDto[] = [];
    isLoading = false;
    visibleIps = new Set<number>();

    // Stats
    totalLogs = 0;
    criticalActions = 0;
    todayLogs = 0;

    // Paginación y Filtros
    currentPage = 1;
    pageSize = 15;

    showDetailModal = false;
    selectedLog: AuditLogResponseDto | null = null;

    ngOnInit() {
        this.loadLogs();
    }

    loadLogs() {
        this.isLoading = true;
        this.auditService.getAllLogs(this.currentPage, this.pageSize).subscribe({
            next: (res) => {
                if (res.succeeded && res.data) {
                    this.logs = res.data;
                    this.totalLogs = this.logs.length > 0 ? this.logs[0].totalRecords : 0;
                    this.calculateStats();
                }
                this.isLoading = false;
                this.cdr.detectChanges();
            }
        });
    }

    calculateStats() {
        // Simulamos stats basadas en la carga actual
        this.criticalActions = this.logs.filter(l => l.actionType.includes('DELETE') || l.actionType.includes('REJECT')).length;
        const today = new Date().toDateString();
        this.todayLogs = this.logs.filter(l => new Date(l.createdAt).toDateString() === today).length;
    }

    applyFilters() {
        this.currentPage = 1;
        this.loadLogs();
    }

    toggleIpVisibility(id: number) {
        this.visibleIps.has(id) ? this.visibleIps.delete(id) : this.visibleIps.add(id);
    }

    getActionClass(action: string): string {
        if (action.includes('CREATE') || action.includes('POST')) return 'bg-emerald-50 text-emerald-700 border-emerald-100';
        if (action.includes('UPDATE') || action.includes('PUT')) return 'bg-blue-50 text-blue-700 border-blue-100';
        if (action.includes('DELETE')) return 'bg-red-50 text-red-700 border-red-100';
        return 'bg-amber-50 text-amber-700 border-amber-100';
    }

    onPageChange(page: number) {
        this.currentPage = page;
        this.loadLogs();
    }


    openDetail(log: AuditLogResponseDto) {
        let formattedMessage = log.message;

        try {
            const jsonStartIndex = log.message.indexOf('{');
            
            if (jsonStartIndex !== -1) {
            const textPart = log.message.substring(0, jsonStartIndex).trim();
            const jsonPart = log.message.substring(jsonStartIndex);
            
            const parsedJson = JSON.parse(jsonPart);
            formattedMessage = `${textPart}\n\n${JSON.stringify(parsedJson, null, 2)}`;
            }
        } 
        catch (e) {
            console.log(e);
        }

        this.selectedLog = { ...log, message: formattedMessage };
        this.showDetailModal = true;
    }
}