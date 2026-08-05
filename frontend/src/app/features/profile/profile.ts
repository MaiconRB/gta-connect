import { HttpClient } from '@angular/common/http';
import { JsonPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';

interface MeResponse {
  id: string;
  email: string;
  displayName: string;
}

@Component({
  selector: 'app-profile',
  imports: [JsonPipe],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.authService.currentUser;

  // Chama /api/auth/me (protegida por [Authorize] no backend) para provar, na prática,
  // que o authInterceptor está anexando o token JWT automaticamente na requisição.
  protected readonly me = signal<MeResponse | null>(null);

  constructor() {
    this.http.get<MeResponse>(`${environment.apiUrl}/auth/me`).subscribe((response) => {
      this.me.set(response);
    });
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
