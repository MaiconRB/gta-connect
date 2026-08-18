import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../common/paged-result.model';
import { ChatMessage, ConversationSummary } from './chat.models';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly http = inject(HttpClient);

  getConversations(): Observable<ConversationSummary[]> {
    return this.http.get<ConversationSummary[]>(`${environment.apiUrl}/conversations`);
  }

  getMessages(conversationId: string, page: number, pageSize: number): Observable<PagedResult<ChatMessage>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<ChatMessage>>(`${environment.apiUrl}/conversations/${conversationId}/messages`, { params });
  }

  getConversationWith(profileId: string): Observable<ConversationSummary> {
    return this.http.get<ConversationSummary>(`${environment.apiUrl}/conversations/with/${profileId}`);
  }
}
