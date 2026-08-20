import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from './auth.models';

const STORAGE_KEY = 'gtaconnect.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  // Signal como fonte única de verdade do estado de sessão — qualquer componente
  // que leia `currentUser`/`isAuthenticated` reage automaticamente a login/logout,
  // sem precisar de Subject/BehaviorSubject manual.
  private readonly authState = signal<AuthResponse | null>(this.readFromStorage());

  readonly currentUser = this.authState.asReadonly();
  readonly isAuthenticated = computed(() => this.authState() !== null);
  readonly isEmailConfirmed = computed(() => this.authState()?.emailConfirmed ?? true);

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.authState.set(null);
  }

  getToken(): string | null {
    return this.authState()?.token ?? null;
  }

  /** Confirma o e-mail com userId + token vindos da query string do link no e-mail. */
  confirmEmail(userId: string, token: string): Observable<void> {
    return this.http
      .get<void>(`${environment.apiUrl}/auth/confirm-email`, {
        params: { userId, token },
      })
      .pipe(tap(() => this.markEmailAsConfirmed()));
  }

  /** Reenvia o e-mail de confirmação para o usuário autenticado. */
  resendConfirmation(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/resend-confirmation`, {});
  }

  private markEmailAsConfirmed(): void {
    const current = this.authState();
    if (!current) return;
    const updated = { ...current, emailConfirmed: true };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    this.authState.set(updated);
  }

  private setSession(response: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    this.authState.set(response);
  }

  private readFromStorage(): AuthResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as AuthResponse;
      if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return parsed;
    } catch {
      return null;
    }
  }
}
