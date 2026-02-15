import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { ApiResponse } from '../../models/api-response';
import { CreateMatchDto, MatchResponseDto, RegisterResultDto, UpdateMatchDto } from '../../../data/features/tournaments/dtos/competition/match.dto';
import { environment } from '../../../../environments/src/environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class MatchService {
  private apiUrl = `${environment.apiUrl}Matches`;

  constructor(private http: HttpClient) { }

  getMatchesByPhaseId(phaseId: number): Observable<ApiResponse<MatchResponseDto[]>> {
    const url = `${this.apiUrl}/phase/${phaseId}`;
    return this.http.get<ApiResponse<MatchResponseDto[]>>(url, { withCredentials: true });
  }

  createMatch(dto: CreateMatchDto): Observable<ApiResponse<number>> {
    if (!dto) {
      return throwError(() => new Error('El objeto CreateMatchDto no puede ser nulo.'));
    }
    return this.http.post<ApiResponse<number>>(this.apiUrl, dto, { withCredentials: true });
  }

  updateMatch(dto: UpdateMatchDto): Observable<ApiResponse<boolean>> {
    if (!dto) {
      return throwError(() => new Error('El objeto UpdateMatchDto no puede ser nulo.'));
    }
    const url = `${this.apiUrl}/${dto.id}`;
    return this.http.put<ApiResponse<boolean>>(url, dto, { withCredentials: true });
  }

  registerResult(dto: RegisterResultDto): Observable<ApiResponse<boolean>> {
    const url = `${this.apiUrl}/result`;
    return this.http.put<ApiResponse<boolean>>(url, dto, { withCredentials: true });
  }

  deleteMatch(id: number): Observable<void> {
    const url = `${this.apiUrl}/${id}`
    return this.http.delete<void>(url, { withCredentials: true });
  }
}