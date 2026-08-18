namespace GtaConnect.Application.Features.Chat;

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderProfileId,
    string Content,
    DateTime SentAtUtc);
