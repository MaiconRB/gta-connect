namespace GtaConnect.Application.Features.Reputation;

public record RatingRequestDto(Guid GameSessionId, int Score, string? Comment, bool CompletedSession, bool KnewWhatToDo, bool WasToxic);
