import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { ApiResponse } from '../../models/api-response';
import { CreatePhaseDto, PhaseResponseDto, UpdatePhaseDto } from '../../../data/features/tournaments/dtos/competition/phase.dto';
import { environment } from '../../../../environments/src/environments/environment.development';


@Injectable({
  providedIn: 'root',
})
export class PhaseService {

  private apiUrl = `${environment.apiUrl}Phases`;

  constructor(private http: HttpClient) { }

  getAllPhases(): Observable<ApiResponse<PhaseResponseDto[]>> {
    return this.http.get<ApiResponse<PhaseResponseDto[]>>(this.apiUrl);
  }

  getPhasesByTournamentId(tournamentId: number): Observable<ApiResponse<PhaseResponseDto[]>> {
    return this.http.get<ApiResponse<PhaseResponseDto[]>>(`${this.apiUrl}/tournament/${tournamentId}`);
  }

  getPhaseById(id: number): Observable<ApiResponse<PhaseResponseDto>> {
    return this.http.get<ApiResponse<PhaseResponseDto>>(`${this.apiUrl}/${id}`);
  }

  createPhase(dto: CreatePhaseDto): Observable<ApiResponse<number>> {
    if (!dto) {
      return throwError(() => new Error('El objeto CreatePhaseDto no puede ser nulo.'));
    }
    return this.http.post<ApiResponse<number>>(this.apiUrl, dto);
  }

  updatePhase(id: number, dto: UpdatePhaseDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  deletePhase(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  reactivatePhase(id: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/entity`, {});
  }

  setFinalPhase(tournamentId: number, phaseId: number): Observable<ApiResponse<boolean>> {
    const url = `${this.apiUrl}/tournament/${tournamentId}/final/${phaseId}`;
    
    return this.http.patch<ApiResponse<boolean>>(url, {});
  }
}