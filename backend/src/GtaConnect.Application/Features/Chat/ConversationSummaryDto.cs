namespace GtaConnect.Application.Features.Chat;

// Um item da lista "minhas conversas" — já traz os dados do OUTRO participante resolvidos
// (nome/avatar), a prévia da última mensagem e a contagem de não-lidas, porque a tela de
// lista precisa de tudo isso de uma vez (evita N+1 chamadas por conversa).
public record ConversationSummaryDto(
    Guid ConversationId,
    Guid OtherProfileId,
    string OtherDisplayName,
    string? OtherAvatarPath,
    string LastMessageContent,
    DateTime LastMessageAtUtc,
    int UnreadCount);
