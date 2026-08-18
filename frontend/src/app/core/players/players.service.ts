import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, PlayerSummary, SearchFilters } from './players.models';

@Injectable({ providedIn: 'root' })
export class PlayersService {
  private readonly http = inject(HttpClient);

  searchPlayers(filters: SearchFilters): Observable<PagedResult<PlayerSummary>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }

    return this.http.get<PagedResult<PlayerSummary>>(`${environment.apiUrl}/players`, { params });
  }

  getPlayerById(id: string): Observable<PlayerSummary> {
    return this.http.get<PlayerSummary>(`${environment.apiUrl}/players/${id}`);
  }
}
