using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Moderation;

public record ReportRequestDto(Guid ProfileId, ReportReason Reason, string? Details);
