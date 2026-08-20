import { Component, OnInit, inject, signal } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { TranslatePipe } from "@ngx-translate/core";
import { AuthService } from "../../../core/auth/auth.service";

type ConfirmState = "loading" | "success" | "error";

@Component({
  selector: "app-confirm-email",
  imports: [TranslatePipe, RouterLink],
  templateUrl: "./confirm-email.html",
})
export class ConfirmEmail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected readonly state = signal<ConfirmState>("loading");
  protected readonly resending = signal(false);
  protected readonly resendSuccess = signal(false);

  ngOnInit(): void {
    const userId = this.route.snapshot.queryParamMap.get("userId");
    const token = this.route.snapshot.queryParamMap.get("token");

    if (!userId || !token) {
      this.state.set("error");
      return;
    }

    this.authService.confirmEmail(userId, token).subscribe({
      next: () => {
        this.state.set("success");
        // Redireciona para o perfil apos 3 segundos.
        setTimeout(() => this.router.navigateByUrl("/perfil"), 3000);
      },
      error: () => this.state.set("error"),
    });
  }

  protected resendConfirmation(): void {
    if (this.resending()) return;
    this.resending.set(true);
    this.authService.resendConfirmation().subscribe({
      next: () => {
        this.resendSuccess.set(true);
        this.resending.set(false);
      },
      error: () => this.resending.set(false),
    });
  }
}
