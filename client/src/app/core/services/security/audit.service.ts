import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/src/environments/environment.development';
import { ApiResponse } from '../../models/api-response';
import { AuditLogResponseDto } from '../../../data/features/tournaments/dtos/security/audit.dto';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}Audit`;

  getAllLogs(pageNumber: number = 1, pageSize: number = 20) {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<ApiResponse<AuditLogResponseDto[]>>(this.apiUrl, { params });
  }
}