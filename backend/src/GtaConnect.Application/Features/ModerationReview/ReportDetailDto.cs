using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.ModerationReview;

public record ReportDetailDto(
    Guid Id,
    string ReporterDisplayName,
    ReportReason Reason,
    string? Details,
    ReportStatus Status,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc);
