import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-empty-state',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './empty-state.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly titleKey = input.required<string>();
  readonly messageKey = input.required<string>();
  readonly ctaKey = input<string | null>(null);
  readonly ctaLink = input<string | null>(null);
}
