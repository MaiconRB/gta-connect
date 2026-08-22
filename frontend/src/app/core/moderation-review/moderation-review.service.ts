import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReportedProfileDetail, ReportedProfileSummary } from './moderation-review.models';

@Injectable({ providedIn: 'root' })
export class ModerationReviewService {
  private readonly http = inject(HttpClient);

  getReportedProfiles(): Observable<ReportedProfileSummary[]> {
    return this.http.get<ReportedProfileSummary[]>(`${environment.apiUrl}/moderation/review`);
  }

  getReportedProfileDetail(profileId: string): Observable<ReportedProfileDetail> {
    return this.http.get<ReportedProfileDetail>(`${environment.apiUrl}/moderation/review/${profileId}`);
  }

  markReportReviewed(reportId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/moderation/review/reports/${reportId}/mark-reviewed`, {});
  }

  ban(profileId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/moderation/review/${profileId}/ban`, {});
  }

  unban(profileId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/moderation/review/${profileId}/unban`, {});
  }

  deletePost(postId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/moderation/review/posts/${postId}`);
  }
}
