import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BlockedProfile, ReportReason } from './moderation.models';

@Injectable({ providedIn: 'root' })
export class ModerationService {
  private readonly http = inject(HttpClient);

  block(profileId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/moderation/blocks`, { profileId });
  }

  unblock(profileId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/moderation/blocks/${profileId}`);
  }

  getBlockedProfiles(): Observable<BlockedProfile[]> {
    return this.http.get<BlockedProfile[]>(`${environment.apiUrl}/moderation/blocks`);
  }

  report(profileId: string, reason: ReportReason, details: string | null): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/moderation/reports`, { profileId, reason, details });
  }
}
