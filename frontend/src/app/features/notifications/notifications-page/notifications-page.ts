import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { AppNotification, NotificationType } from '../../../core/notifications/notifications.models';
import { NotificationsService } from '../../../core/notifications/notifications.service';
import { PlayerAvatarComponent } from '../../../shared/player-avatar/player-avatar';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';
import { LoadingTextComponent } from '../../../shared/loading-text/loading-text';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-notifications-page',
  imports: [RouterLink, TranslatePipe, PlayerAvatarComponent, ErrorMessageComponent, LoadingTextComponent],
  templateUrl: './notifications-page.html',
  styleUrl: './notifications-page.css',
})
export class NotificationsPage {
  private readonly notificationsService = inject(NotificationsService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly NotificationType = NotificationType;

  protected readonly notifications = signal<AppNotification[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isMarkingAll = signal(false);

  constructor() {
    this.loadNotifications();
  }

  protected open(notification: AppNotification): void {
    if (!notification.isRead) {
      this.notificationsService.markAsRead(notification.id).subscribe({
        next: () => {
          this.notifications.update((current) =>
            current.map((n) => (n.id === notification.id ? { ...n, isRead: true } : n)),
          );
          this.notificationsService.notifyChanged();
        },
        error: (error: HttpErrorResponse) => this.toast.error(extractErrorMessage(error)),
      });
    }

    this.router.navigate(this.targetRoute(notification));
  }

  protected markAllAsRead(): void {
    this.isMarkingAll.set(true);
    this.notificationsService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.update((current) => current.map((n) => ({ ...n, isRead: true })));
        this.isMarkingAll.set(false);
        this.notificationsService.notifyChanged();
      },
      error: (error: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(error));
        this.isMarkingAll.set(false);
      },
    });
  }

  protected hasUnread(): boolean {
    return this.notifications().some((n) => !n.isRead);
  }

  private targetRoute(notification: AppNotification): unknown[] {
    switch (notification.type) {
      case NotificationType.ConnectionRequestReceived:
        return ['/conexoes'];
      case NotificationType.MessageReceived:
        return ['/mensagens/with', notification.actorProfileId];
      case NotificationType.RatingReceived:
        return ['/perfil'];
      case NotificationType.PostLiked:
        return ['/feed'];
      case NotificationType.SessionLogged:
        return ['/conexoes'];
    }
  }

  private loadNotifications(): void {
    this.isLoading.set(true);
    this.notificationsService.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications.set(notifications);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
