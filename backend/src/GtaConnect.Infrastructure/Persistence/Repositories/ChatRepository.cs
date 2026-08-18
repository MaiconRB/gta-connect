using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Chat;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _dbContext;

    public ChatRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Conversation?> FindConversationAsync(Guid participantAId, Guid participantBId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Conversations
            .SingleOrDefaultAsync(c => c.ParticipantAId == participantAId && c.ParticipantBId == participantBId, cancellationToken);
    }

    public Task<Conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Conversations.SingleOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
    }

    public async Task SaveNewMessageAsync(Conversation conversation, Message message, bool isNewConversation, CancellationToken cancellationToken = default)
    {
        if (isNewConversation)
        {
            _dbContext.Conversations.Add(conversation);
        }

        // Quando NÃO é nova, "conversation" já veio rastreado por FindConversationAsync
        // (mesmo DbContext/escopo) — o change tracker do EF Core já sabe sozinho que só
        // LastMessageAtUtc mudou e gera um UPDATE parcial. Chamar Conversations.Update()
        // aqui marcaria a entidade INTEIRA como modificada, sobrescrevendo colunas como
        // ParticipantXLastReadAtUtc com o valor que estava em memória nesta instância —
        // que pode estar desatualizado se outra invocação do Hub (ex: MarkAsRead rodando
        // em paralelo, em outro DbContext) mudou essa coluna nesse meio-tempo. Foi
        // exatamente esse "last write wins" que perdia o MarkAsRead de quem tinha acabado
        // de receber a mensagem (bug encontrado e corrigido na verificação end-to-end).

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveReadStatusAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        // Mesmo motivo do comentário em SaveNewMessageAsync — "conversation" já é
        // rastreado (veio de GetConversationByIdAsync), então nada de Update() aqui.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsForProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        // Toda conversa listada aqui tem, por invariante, pelo menos uma mensagem — a única
        // forma de criar uma Conversation é via SaveNewMessageAsync, sempre junto da primeira
        // Message. Por isso lastMessage.First() é seguro (não precisa de FirstOrDefault/null-check).
        var query =
            from c in _dbContext.Conversations
            where c.ParticipantAId == profileId || c.ParticipantBId == profileId
            let otherProfileId = c.ParticipantAId == profileId ? c.ParticipantBId : c.ParticipantAId
            let myLastReadAtUtc = c.ParticipantAId == profileId ? c.ParticipantALastReadAtUtc : c.ParticipantBLastReadAtUtc
            join other in _dbContext.PlayerProfiles on otherProfileId equals other.Id
            let lastMessage = _dbContext.Messages
                .Where(m => m.ConversationId == c.Id)
                .OrderByDescending(m => m.SentAtUtc)
                .First()
            orderby c.LastMessageAtUtc descending
            select new ConversationSummaryDto(
                c.Id,
                other.Id,
                other.DisplayName,
                other.AvatarPath,
                lastMessage.Content,
                lastMessage.SentAtUtc,
                _dbContext.Messages.Count(m => m.ConversationId == c.Id
                    && m.SenderProfileId != profileId
                    && (myLastReadAtUtc == null || m.SentAtUtc > myLastReadAtUtc)));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Message> Items, int TotalCount)> GetMessagesAsync(Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Messages.Where(m => m.ConversationId == conversationId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.SentAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
