import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ConnectionStatusInfo, ConnectionSummary } from './social.models';

@Injectable({ providedIn: 'root' })
export class SocialService {
  private readonly http = inject(HttpClient);

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

  /// Registra "jogamos juntos agora" pra uma conexão aceita. Devolve o id da sessão criada —
  /// é ela que a avaliação (rate) referencia depois.
  logSession(connectionId: string): Observable<string> {
    return this.http.post<string>(`${environment.apiUrl}/game-sessions`, { connectionId });
  }

  rate(
    gameSessionId: string,
    score: number,
    comment: string | null,
    completedSession: boolean,
    knewWhatToDo: boolean,
    wasToxic: boolean,
  ): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/ratings`, {
      gameSessionId,
      score,
      comment,
      completedSession,
      knewWhatToDo,
      wasToxic,
    });
  }
}
