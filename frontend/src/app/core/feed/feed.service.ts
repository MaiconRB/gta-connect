import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../common/paged-result.model';
import { LikeToggleResult, Post } from './feed.models';

@Injectable({ providedIn: 'root' })
export class FeedService {
  private readonly http = inject(HttpClient);

  getFeed(page: number, pageSize: number, onlyConnections = false): Observable<PagedResult<Post>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize).set('onlyConnections', onlyConnections);
    return this.http.get<PagedResult<Post>>(`${environment.apiUrl}/feed`, { params });
  }

  createPost(content: string | null, photoFile: File | null): Observable<Post> {
    const formData = new FormData();
    if (content) {
      formData.append('content', content);
    }
    if (photoFile) {
      formData.append('photo', photoFile);
    }
    return this.http.post<Post>(`${environment.apiUrl}/feed/posts`, formData);
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/feed/posts/${id}`);
  }

  toggleLike(id: string): Observable<LikeToggleResult> {
    return this.http.post<LikeToggleResult>(`${environment.apiUrl}/feed/posts/${id}/like`, {});
  }
}
