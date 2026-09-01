import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastService } from '../../core/toast/toast.service';

// Montado uma vez em app.html (fora do <router-outlet>), sobrepõe qualquer rota.
@Component({
  selector: 'app-toast-container',
  imports: [TranslatePipe],
  templateUrl: './toast-container.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);
}
