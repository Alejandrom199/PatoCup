import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/src/environments/environment.development';
import { Observable } from 'rxjs';
import { throwError } from 'rxjs'; // Importar throwError
import { ApiResponse } from '../../models/api-response'; // Assuming this file exists and is correctly defined
import { CreateTournament, TournamentBracketDto, TournamentQuery, TournamentResponseDto, UpdateTournamentDto } from '../../../data/features/tournaments/dtos/competition/tournament.dto';

@Injectable({
  providedIn: 'root',
})
export class TournamentService {

  private apiUrl = `${environment.apiUrl}Tournaments/`;

  constructor(private http: HttpClient) { }

  getAllTournaments(query: TournamentQuery): Observable<ApiResponse<TournamentResponseDto[]>> {
    let params = new HttpParams()
      .set('PageNumber', query.pageNumber?.toString() ?? '1')
      .set('PageSize', query.pageSize?.toString() ?? '10');

    if (query.name && query.name.trim() !== '') {
      params = params.set('Name', query.name);
    }

    if (query.tournamentStateId) {
      params = params.set('TournamentStateId', query.tournamentStateId.toString());
    }

    return this.http.get<ApiResponse<TournamentResponseDto[]>>(this.apiUrl, { params }, );
  }

  getTournamentById(id: number): Observable<ApiResponse<TournamentResponseDto>> {
    const url = `${this.apiUrl}${id}`;
    return this.http.get<ApiResponse<TournamentResponseDto>>(url, { withCredentials: true });
  }

  createTournament(dto: CreateTournament): Observable<ApiResponse<number>> {
    if (!dto) {
      return throwError(() => new Error('El objeto createTournamentDto no puede ser nulo o indefinido.'));
    }
    return this.http.post<ApiResponse<number>>(this.apiUrl, dto, { withCredentials: true });
  }

  updateTournament(dto: UpdateTournamentDto): Observable<ApiResponse<boolean>> {
    if (!dto) {
      return throwError(() => new Error('El objeto UpdateTournamentDto no puede ser nulo.'));
    }
    const url = `${this.apiUrl}${dto.id}`
    return this.http.put<ApiResponse<boolean>>(url, dto, { withCredentials: true });
  }

  deleteTournament(id: number): Observable<ApiResponse<boolean>> {
    if (!id) {
      return throwError(() => new Error('El id no puede ser nulo o indefinido.'));
    }
    const url = `${this.apiUrl}${id}`
    return this.http.delete<ApiResponse<boolean>>(url, { withCredentials: true });
  }

  reactivateTournament(id: number): Observable<ApiResponse<boolean>> {
    if (!id){
      return throwError(() => new Error('El id no puede ser nulo o indefinido.'));
    }
    const url = `${this.apiUrl}reactivate/${id}`;
    return this.http.put<ApiResponse<boolean>>(url, { withCredentials: true });
  }

  setPublicTournament(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}${id}/set-public`, {}, { withCredentials: true });
  }

  getPublicTournament(): Observable<ApiResponse<TournamentResponseDto>> {
    return this.http.get<ApiResponse<TournamentResponseDto>>(`${this.apiUrl}public`);
  }

  getPublicBracket(): Observable<ApiResponse<TournamentBracketDto>> {
    return this.http.get<ApiResponse<TournamentBracketDto>>(`${this.apiUrl}public-bracket`);
  }
}
