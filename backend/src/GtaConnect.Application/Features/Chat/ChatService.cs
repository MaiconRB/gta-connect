using GtaConnect.Application.Common;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Features.Chat;

public class ChatService : IChatService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    private readonly IChatRepository _chatRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;

    public ChatService(IChatRepository chatRepository, IPlayerProfileRepository playerProfileRepository)
    {
        _chatRepository = chatRepository;
        _playerProfileRepository = playerProfileRepository;
    }

    public async Task<SendMessageResultDto> SendMessageAsync(Guid senderUserId, Guid recipientProfileId, string content, CancellationToken cancellationToken = default)
    {
        var senderProfile = await _playerProfileRepository.GetByUserIdAsync(senderUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), senderUserId);

        var recipientProfile = await _playerProfileRepository.GetByIdAsync(recipientProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), recipientProfileId);

        var (participantAId, participantBId) = Conversation.NormalizeParticipantOrder(senderProfile.Id, recipientProfile.Id);
        var existingConversation = await _chatRepository.FindConversationAsync(participantAId, participantBId, cancellationToken);

        var isNewConversation = existingConversation is null;
        var conversation = existingConversation ?? Conversation.Create(senderProfile.Id, recipientProfile.Id);

        var message = conversation.PostMessage(senderProfile.Id, content);

        await _chatRepository.SaveNewMessageAsync(conversation, message, isNewConversation, cancellationToken);

        return new SendMessageResultDto(ToMessageDto(message), senderProfile.ApplicationUserId, recipientProfile.ApplicationUserId);
    }

    public async Task<IReadOnlyList<ConversationSummaryDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        return await _chatRepository.GetConversationsForProfileAsync(profile.Id, cancellationToken);
    }

    public async Task<PagedResultDto<MessageDto>> GetMessagesAsync(Guid userId, Guid conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var conversation = await GetConversationForParticipantOrThrowAsync(conversationId, profile.Id, cancellationToken);

        var clampedPage = page < 1 ? 1 : page;
        var clampedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var (items, totalCount) = await _chatRepository.GetMessagesAsync(conversation.Id, clampedPage, clampedPageSize, cancellationToken);

        return new PagedResultDto<MessageDto>(items.Select(ToMessageDto).ToList(), totalCount, clampedPage, clampedPageSize);
    }

    public async Task<ConversationSummaryDto?> GetConversationWithAsync(Guid userId, Guid otherProfileId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        // Lista pequena por usuário no volume do MVP — reaproveitar a mesma projeção da
        // listagem evita duplicar a query de "resolver conversa + dados do outro participante".
        var conversations = await _chatRepository.GetConversationsForProfileAsync(profile.Id, cancellationToken);
        return conversations.FirstOrDefault(c => c.OtherProfileId == otherProfileId);
    }

    public async Task MarkAsReadAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var conversation = await GetConversationForParticipantOrThrowAsync(conversationId, profile.Id, cancellationToken);

        conversation.MarkReadBy(profile.Id, DateTime.UtcNow);
        await _chatRepository.SaveReadStatusAsync(conversation, cancellationToken);
    }

    private async Task<PlayerProfile> GetProfileOrThrowAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);
    }

    // "Não existe" cobre tanto "conversa não existe" quanto "existe mas não é sua" — não
    // confirma pra quem pergunta se uma conversa alheia existe (mesmo padrão do PlayersController).
    private async Task<Conversation> GetConversationForParticipantOrThrowAsync(Guid conversationId, Guid profileId, CancellationToken cancellationToken)
    {
        var conversation = await _chatRepository.GetConversationByIdAsync(conversationId, cancellationToken);
        if (conversation is null || !conversation.HasParticipant(profileId))
        {
            throw new NotFoundException(nameof(Conversation), conversationId);
        }

        return conversation;
    }

    private static MessageDto ToMessageDto(Message message) => new(
        message.Id,
        message.ConversationId,
        message.SenderProfileId,
        message.Content,
        message.SentAtUtc);
}
