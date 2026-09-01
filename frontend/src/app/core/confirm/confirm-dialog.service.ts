import { Injectable, inject } from '@angular/core';
import { Dialog } from '@angular/cdk/dialog';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { ConfirmDialogComponent, ConfirmDialogData } from './confirm-dialog.component';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
}

// Confirmação padrão pra ações destrutivas (banir, apagar post, bloquear...) — nenhuma
// tinha confirmação antes (ver auditoria/plano da Fase 4). CDK Dialog (não Material):
// traz focus-trap e retorno de foco de graça, sem opinião de estilo própria — skinado
// 100% com o Tailwind já existente no ConfirmDialogComponent.
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly dialog = inject(Dialog);
  private readonly translate = inject(TranslateService);

  confirm(options: ConfirmOptions): Promise<boolean> {
    const data: ConfirmDialogData = {
      title: options.title,
      message: options.message,
      confirmLabel: options.confirmLabel ?? this.translate.instant('common.confirm'),
      cancelLabel: options.cancelLabel ?? this.translate.instant('common.cancel'),
      danger: options.danger ?? false,
    };

    const ref = this.dialog.open<boolean, ConfirmDialogData>(ConfirmDialogComponent, {
      data,
      panelClass: 'flex items-center justify-center',
      backdropClass: 'bg-neutral-950/70',
    });

    return firstValueFrom(ref.closed).then((result) => result ?? false);
  }
}
