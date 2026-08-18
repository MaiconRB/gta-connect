using GtaConnect.Application.Features.Chat;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

/// <summary>
/// Cobre Conversation e Message juntos, de propósito — toda escrita relevante (mandar
/// mensagem) sempre toca as duas ao mesmo tempo, e isso precisa acontecer numa única
/// SaveChangesAsync (evita persistir a mensagem sem atualizar a conversa, ou vice-versa).
/// </summary>
public interface IChatRepository
{
    Task<Conversation?> FindConversationAsync(Guid participantAId, Guid participantBId, CancellationToken cancellationToken = default);

    Task<Conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>Persiste a conversa (só se for nova) e a mensagem numa única SaveChangesAsync.</summary>
    Task SaveNewMessageAsync(Conversation conversation, Message message, bool isNewConversation, CancellationToken cancellationToken = default);

    Task SaveReadStatusAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>Lista de conversas do perfil informado, com dados do outro participante, prévia da última mensagem e contagem de não-lidas já resolvidos.</summary>
    Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsForProfileAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Message> Items, int TotalCount)> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
}
