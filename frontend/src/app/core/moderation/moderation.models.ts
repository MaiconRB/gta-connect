export interface BlockedProfile {
  blockedProfileId: string;
  displayName: string;
  avatarPath: string | null;
  blockedAtUtc: string;
}

// Espelha GtaConnect.Domain.Enums.ReportReason (seleção única, não [Flags]).
export enum ReportReason {
  Toxicidade = 1,
  Spam = 2,
  Assedio = 3,
  Outro = 4,
}

export const REPORT_REASON_OPTIONS: { value: ReportReason; labelKey: string }[] = [
  { value: ReportReason.Toxicidade, labelKey: 'moderation.reportReasons.toxicidade' },
  { value: ReportReason.Spam, labelKey: 'moderation.reportReasons.spam' },
  { value: ReportReason.Assedio, labelKey: 'moderation.reportReasons.assedio' },
  { value: ReportReason.Outro, labelKey: 'moderation.reportReasons.outro' },
];
