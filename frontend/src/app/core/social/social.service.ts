import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConnectionStatusInfo, ConnectionSummary } from './social.models';

@Injectable({ providedIn: 'root' })
export class SocialService {
  private readonly http = inject(HttpClient);

  // Sem SignalR pra conexões — componentes chamam notifyChanged() após uma ação
  // bem-sucedida (pedido/aceite/recusa/cancelamento) pra que o badge de pedidos
  // pendentes na navegação (app.ts) saiba que precisa recarregar a contagem.
  readonly connectionsChanged = signal(0);

  notifyChanged(): void {
    this.connectionsChanged.update((value) => value + 1);
  }

  sendRequest(profileId: string): Observable<ConnectionSummary> {
    return this.http.post<ConnectionSummary>(`${environment.apiUrl}/connections`, { profileId });
  }

  accept(connectionId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/connections/${connectionId}/accept`, {});
  }

  decline(connectionId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/connections/${connectionId}/decline`, {});
  }

  getConnections(): Observable<ConnectionSummary[]> {
    return this.http.get<ConnectionSummary[]>(`${environment.apiUrl}/connections`);
  }

  getConnectionStatus(profileId: string): Observable<ConnectionStatusInfo> {
    return this.http.get<ConnectionStatusInfo>(`${environment.apiUrl}/connections/with/${profileId}`);
  }

  rate(profileId: string, score: number, comment: string | null): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/ratings`, { profileId, score, comment });
  }
}
