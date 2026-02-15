import { Injectable } from '@angular/core';
import { environment } from '../../../../environments/src/environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/api-response';
import { Catalog as CatalogStatus } from '../../models/catalog';

@Injectable({
  providedIn: 'root',
})
export class CatalogService {
  
  private readonly API_URL = `${environment.apiUrl}Catalogs`;

  constructor(private http: HttpClient){}

  getGeneralStatus(): Observable<ApiResponse<CatalogStatus[]>> {
    return this.http.get<ApiResponse<CatalogStatus[]>>(`${this.API_URL}/general-status`);
  }

  getMatchStatus(): Observable<ApiResponse<CatalogStatus[]>> {
    return this.http.get<ApiResponse<CatalogStatus[]>>(`${this.API_URL}/match-status`);
  }

  getPhaseStatus(): Observable<ApiResponse<CatalogStatus[]>> {
    return this.http.get<ApiResponse<CatalogStatus[]>>(`${this.API_URL}/phase-status`);
  }

  getPlayerStatus(): Observable<ApiResponse<CatalogStatus[]>> {
    return this.http.get<ApiResponse<CatalogStatus[]>>(`${this.API_URL}/player-status`);
  }

  getTournamentStatus(): Observable<ApiResponse<CatalogStatus[]>> {
    return this.http.get<ApiResponse<CatalogStatus[]>>(`${this.API_URL}/tournament-status`);
  }
}
