import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { calculateCompleteness } from '../../../core/profile/profile-completeness.util';
import { ProfileResponse } from '../../../core/profile/profile.models';

// Componente "burro": só calcula e mostra, não injeta nenhum serviço — quem decide o que
// fazer com o clique no CTA é o pai (Profile.startEdit()), via output. Fica dentro de
// features/profile/ (não shared/) porque a fórmula é específica de ProfileResponse, não
// reutilizada em outro lugar hoje — evita abstração prematura.
@Component({
  selector: 'app-profile-completion-bar',
  imports: [TranslatePipe],
  templateUrl: './profile-completion-bar.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileCompletionBarComponent {
  readonly profile = input.required<ProfileResponse>();
  readonly edit = output<void>();

  protected readonly percentage = computed(() => calculateCompleteness(this.profile()));
}
