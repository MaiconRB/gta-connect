import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { extractErrorMessage } from '../../../core/http/problem-details.util';
import { FeedService } from '../../../core/feed/feed.service';
import { Post } from '../../../core/feed/feed.models';
import { ProfileService } from '../../../core/profile/profile.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-feed-page',
  imports: [FormsModule, TranslatePipe, DatePipe],
  templateUrl: './feed-page.html',
  styleUrl: './feed-page.css',
})
export class FeedPage {
  private readonly feedService = inject(FeedService);
  private readonly profileService = inject(ProfileService);

  private nextPage = 1;
  protected readonly myProfileId = signal<string | null>(null);
  protected readonly onlyConnections = signal(false);

  protected readonly posts = signal<Post[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly hasMore = signal(false);
  protected readonly isLoadingMore = signal(false);

  protected readonly draft = signal('');
  protected readonly selectedPhoto = signal<File | null>(null);
  protected readonly photoPreviewUrl = signal<string | null>(null);
  protected readonly isPosting = signal(false);
  protected readonly postError = signal<string | null>(null);

  protected readonly togglingLikeId = signal<string | null>(null);
  protected readonly deletingId = signal<string | null>(null);

  constructor() {
    this.profileService.getMyProfile().subscribe({
      next: (profile) => this.myProfileId.set(profile.id),
    });

    this.loadFeed();
  }

  protected onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.postError.set(null);

    if (this.photoPreviewUrl()) {
      URL.revokeObjectURL(this.photoPreviewUrl()!);
    }

    if (!file) {
      this.selectedPhoto.set(null);
      this.photoPreviewUrl.set(null);
      return;
    }

    this.selectedPhoto.set(file);
    this.photoPreviewUrl.set(URL.createObjectURL(file));
  }

  protected clearPhoto(): void {
    if (this.photoPreviewUrl()) {
      URL.revokeObjectURL(this.photoPreviewUrl()!);
    }
    this.selectedPhoto.set(null);
    this.photoPreviewUrl.set(null);
  }

  protected submitPost(): void {
    const content = this.draft().trim();
    const photo = this.selectedPhoto();
    if (!content && !photo) {
      return;
    }

    this.isPosting.set(true);
    this.postError.set(null);

    this.feedService.createPost(content === '' ? null : content, photo).subscribe({
      next: (post) => {
        this.posts.update((current) => [post, ...current]);
        this.draft.set('');
        this.clearPhoto();
        this.isPosting.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.postError.set(extractErrorMessage(error));
        this.isPosting.set(false);
      },
    });
  }

  protected toggleLike(post: Post): void {
    this.togglingLikeId.set(post.id);
    this.feedService.toggleLike(post.id).subscribe({
      next: (result) => {
        this.posts.update((current) =>
          current.map((p) => (p.id === post.id ? { ...p, likedByMe: result.liked, likeCount: result.likeCount } : p)),
        );
        this.togglingLikeId.set(null);
      },
      error: () => {
        this.togglingLikeId.set(null);
      },
    });
  }

  protected deletePost(post: Post): void {
    this.deletingId.set(post.id);
    this.feedService.deletePost(post.id).subscribe({
      next: () => {
        this.posts.update((current) => current.filter((p) => p.id !== post.id));
        this.deletingId.set(null);
      },
      error: () => {
        this.deletingId.set(null);
      },
    });
  }

  protected loadMore(): void {
    this.nextPage += 1;
    this.isLoadingMore.set(true);

    this.feedService.getFeed(this.nextPage, PAGE_SIZE, this.onlyConnections()).subscribe({
      next: (result) => {
        this.posts.update((current) => [...current, ...result.items]);
        this.hasMore.set(this.nextPage * PAGE_SIZE < result.totalCount);
        this.isLoadingMore.set(false);
      },
      error: () => {
        this.isLoadingMore.set(false);
      },
    });
  }

  protected setOnlyConnections(value: boolean): void {
    if (this.onlyConnections() === value) {
      return;
    }
    this.onlyConnections.set(value);
    this.loadFeed();
  }

  private loadFeed(): void {
    this.nextPage = 1;
    this.isLoading.set(true);
    this.feedService.getFeed(1, PAGE_SIZE, this.onlyConnections()).subscribe({
      next: (result) => {
        this.posts.set(result.items);
        this.hasMore.set(PAGE_SIZE < result.totalCount);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }
}
