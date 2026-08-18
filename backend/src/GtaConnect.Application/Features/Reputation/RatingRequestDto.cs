namespace GtaConnect.Application.Features.Reputation;

public record RatingRequestDto(Guid ProfileId, int Score, string? Comment);
