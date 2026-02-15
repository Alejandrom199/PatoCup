import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/src/environments/environment.development';
import { PlayerResponseDto, PlayerSelectDto, UpdatePlayerDto } from '../../../data/features/tournaments/dtos/competition/player.dto';
import { ApiResponse } from '../../models/api-response';


@Injectable({ providedIn: 'root' })
export class PlayerService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}Players`;

  // Listado para la tabla con filtros
  getAllPlayers(pageNumber: number, pageSize: number, filter?: string) {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (filter) params = params.set('filter', filter);

    return this.http.get<ApiResponse<PlayerResponseDto[]>>(this.apiUrl, { params });
  }

  // Para el combo de los Matches
  getPlayersSelect() {
    return this.http.get<ApiResponse<PlayerSelectDto[]>>(`${this.apiUrl}/select`);
  }

  // PATCH para Aceptar/Rechazar (statusId 2 o 3)
  processRequest(id: number, statusId: number) {
    return this.http.patch<ApiResponse<string>>(`${this.apiUrl}/${id}/process?statusId=${statusId}`, {});
  }

  updatePlayer(id: number, dto: UpdatePlayerDto) {
    return this.http.put(`${this.apiUrl}/${id}`, dto);
  }

  deletePlayer(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  publicSubmit(dto: any) {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/public/submit`, dto);
  }
}