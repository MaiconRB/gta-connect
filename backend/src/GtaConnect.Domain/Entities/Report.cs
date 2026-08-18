using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Entities;

public class Report
{
    private const int MaxDetailsLength = 500;

    public Guid Id { get; private set; }

    public Guid ReporterProfileId { get; private set; }

    public Guid ReportedProfileId { get; private set; }

    public ReportReason Reason { get; private set; }

    public string? Details { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Report()
    {
    }

    public static Report Create(Guid reporterProfileId, Guid reportedProfileId, ReportReason reason, string? details)
    {
        if (reporterProfileId == Guid.Empty || reportedProfileId == Guid.Empty)
        {
            throw new ArgumentException("Os perfis da denúncia não podem ser vazios.");
        }

        if (reporterProfileId == reportedProfileId)
        {
            throw new ArgumentException("Não é possível denunciar a si mesmo.");
        }

        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentException("Motivo de denúncia inválido.", nameof(reason));
        }

        var trimmedDetails = string.IsNullOrWhiteSpace(details) ? null : details.Trim();
        if (trimmedDetails is { Length: > MaxDetailsLength })
        {
            throw new ArgumentException($"Detalhes não pode ter mais de {MaxDetailsLength} caracteres.", nameof(details));
        }

        return new Report
        {
            Id = Guid.NewGuid(),
            ReporterProfileId = reporterProfileId,
            ReportedProfileId = reportedProfileId,
            Reason = reason,
            Details = trimmedDetails,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
