import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppNotification } from './notifications.models';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly http = inject(HttpClient);

  // Sino no header não tem como saber sozinho quando a notifications-page marcou algo
  // como lida — ela chama notifyChanged() depois de uma ação bem-sucedida, e app.ts
  // reage recarregando a contagem de não-lidas.
  readonly notificationsChanged = signal(0);

  notifyChanged(): void {
    this.notificationsChanged.update((value) => value + 1);
  }

  getNotifications(): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(`${environment.apiUrl}/notifications`);
  }

  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${environment.apiUrl}/notifications/unread-count`);
  }

  markAsRead(notificationId: string): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/notifications/${notificationId}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/notifications/read-all`, {});
  }
}
