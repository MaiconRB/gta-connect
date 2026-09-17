import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, Validators, NonNullableFormBuilder } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { ErrorMessageComponent } from '../../../shared/error-message/error-message';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, ErrorMessageComponent],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.router.navigateByUrl('/jogadores');
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(extractErrorMessage(error));
        this.isSubmitting.set(false);
      },
    });
  }
}
