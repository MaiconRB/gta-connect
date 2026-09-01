namespace GtaConnect.Application.Features.Reputation;

public record GameSessionSummaryDto(Guid Id, Guid ConnectionId, DateTime PlayedAtUtc, bool RatedByMe);
