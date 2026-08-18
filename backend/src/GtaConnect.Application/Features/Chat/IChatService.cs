using GtaConnect.Application.Common;

namespace GtaConnect.Application.Features.Chat;

public interface IChatService
{
    /// <summary>Manda uma mensagem pro perfil informado — cria a conversa na primeira vez, reaproveita depois.</summary>
    Task<SendMessageResultDto> SendMessageAsync(Guid senderUserId, Guid recipientProfileId, string content, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<MessageDto>> GetMessagesAsync(Guid userId, Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Null quando ainda não existe conversa entre os dois — vira 404 no controller.</summary>
    Task<ConversationSummaryDto?> GetConversationWithAsync(Guid userId, Guid otherProfileId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
}
