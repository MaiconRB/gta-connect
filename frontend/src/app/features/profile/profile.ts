import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { extractErrorMessage } from '../../core/http/problem-details.util';
import { PLAYSTYLE_TAG_OPTIONS, PlaystyleTag, ProfileResponse } from '../../core/profile/profile.models';
import { ProfileService } from '../../core/profile/profile.service';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(NonNullableFormBuilder);

  protected readonly playstyleTagOptions = PLAYSTYLE_TAG_OPTIONS;

  protected readonly profile = signal<ProfileResponse | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);

  protected readonly mode = signal<'view' | 'edit'>('view');
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly selectedTags = signal<Set<PlaystyleTag>>(new Set());

  protected readonly selectedAvatarFile = signal<File | null>(null);
  protected readonly avatarPreviewUrl = signal<string | null>(null);
  protected readonly isUploadingAvatar = signal(false);
  protected readonly avatarError = signal<string | null>(null);

  protected readonly form = this.formBuilder.group({
    bio: ['', [Validators.maxLength(500)]],
    hoursPlayed: [0, [Validators.min(0), Validators.max(100_000)]],
    favoriteModes: ['', [Validators.maxLength(200)]],
  });

  constructor() {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.isLoading.set(true);
    this.profileService.getMyProfile().subscribe({
      next: (response) => {
        this.profile.set(response);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  protected activeTagLabels(playstyleTags: number): string[] {
    return this.playstyleTagOptions.filter((option) => (playstyleTags & option.value) !== 0).map((option) => option.labelKey);
  }

  protected startEdit(): void {
    const current = this.profile();
    if (!current) {
      return;
    }

    this.form.setValue({
      bio: current.bio ?? '',
      hoursPlayed: current.hoursPlayed,
      favoriteModes: current.favoriteModes ?? '',
    });

    const tags = this.playstyleTagOptions.filter((option) => (current.playstyleTags & option.value) !== 0).map((option) => option.value);
    this.selectedTags.set(new Set(tags));

    this.errorMessage.set(null);
    this.mode.set('edit');
  }

  protected cancelEdit(): void {
    this.mode.set('view');
    this.errorMessage.set(null);
  }

  protected toggleTag(tag: PlaystyleTag): void {
    const updated = new Set(this.selectedTags());
    if (updated.has(tag)) {
      updated.delete(tag);
    } else {
      updated.add(tag);
    }
    this.selectedTags.set(updated);
  }

  protected isTagSelected(tag: PlaystyleTag): boolean {
    return this.selectedTags().has(tag);
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { bio, hoursPlayed, favoriteModes } = this.form.getRawValue();
    const playstyleTags = Array.from(this.selectedTags()).reduce((mask, tag) => mask | tag, 0);

    this.profileService
      .updateMyProfile({
        bio: bio.trim() === '' ? null : bio,
        hoursPlayed,
        favoriteModes: favoriteModes.trim() === '' ? null : favoriteModes,
        playstyleTags,
      })
      .subscribe({
        next: (response) => {
          this.profile.set(response);
          this.isSubmitting.set(false);
          this.mode.set('view');
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          this.isSubmitting.set(false);
        },
      });
  }

  protected onAvatarFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.avatarError.set(null);

    if (this.avatarPreviewUrl()) {
      URL.revokeObjectURL(this.avatarPreviewUrl()!);
    }

    if (!file) {
      this.selectedAvatarFile.set(null);
      this.avatarPreviewUrl.set(null);
      return;
    }

    this.selectedAvatarFile.set(file);
    this.avatarPreviewUrl.set(URL.createObjectURL(file));
  }

  protected uploadAvatar(): void {
    const file = this.selectedAvatarFile();
    if (!file) {
      return;
    }

    this.isUploadingAvatar.set(true);
    this.avatarError.set(null);

    this.profileService.uploadAvatar(file).subscribe({
      next: (response) => {
        this.profile.set(response);
        this.isUploadingAvatar.set(false);
        this.selectedAvatarFile.set(null);
        if (this.avatarPreviewUrl()) {
          URL.revokeObjectURL(this.avatarPreviewUrl()!);
        }
        this.avatarPreviewUrl.set(null);
      },
      error: (error: HttpErrorResponse) => {
        this.avatarError.set(extractErrorMessage(error));
        this.isUploadingAvatar.set(false);
      },
    });
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
