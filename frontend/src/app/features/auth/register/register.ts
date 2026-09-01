import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, Validators, NonNullableFormBuilder } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../../core/auth/auth.service';
import { GameTitle, Platform } from '../../../core/auth/auth.models';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, ErrorMessageComponent],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly Platform = Platform;
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.formBuilder.group({
    displayName: ['', [Validators.required, Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    platform: [Platform.Ps5, [Validators.required]],
  });

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { displayName, email, password, platform } = this.form.getRawValue();

    this.authService
      .register({ displayName, email, password, platform, gameTitle: GameTitle.GtaV })
      .subscribe({
        next: () => {
          // Cadastro novo entra direto no fluxo de completar perfil (Profile abre em modo
          // edit quando vê esse query param) — perfil nasce zerado, sem isso o usuário só
          // acha o form de edição se clicar "Editar" por conta própria.
          this.router.navigate(['/perfil'], { queryParams: { onboarding: '1' } });
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          this.isSubmitting.set(false);
        },
      });
  }
}
