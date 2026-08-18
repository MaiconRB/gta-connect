namespace GtaConnect.Domain.Enums;

// Seleção única (não [Flags]) — uma denúncia tem um motivo principal, não uma combinação.
public enum ReportReason
{
    Toxicidade = 1,
    Spam = 2,
    Assedio = 3,
    Outro = 4,
}
