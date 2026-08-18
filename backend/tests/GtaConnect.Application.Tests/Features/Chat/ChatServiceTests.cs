using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Chat;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Chat;

public class ChatServiceTests
{
    private readonly Mock<IChatRepository> _chatRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly ChatService _sut;

    public ChatServiceTests()
    {
        _sut = new ChatService(_chatRepositoryMock.Object, _playerProfileRepositoryMock.Object);
    }

    private static PlayerProfile CreateValidProfile(Guid userId, string displayName = "Jogador") =>
        PlayerProfile.Create(userId, displayName, Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public async Task SendMessageAsync_PrimeiraMensagemEntreOsDois_CriaConversaNova()
    {
        var senderUserId = Guid.NewGuid();
        var senderProfile = CreateValidProfile(senderUserId, "Remetente");
        var recipientProfile = CreateValidProfile(Guid.NewGuid(), "Destinatario");

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>())).ReturnsAsync(senderProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(recipientProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(recipientProfile);
        _chatRepositoryMock.Setup(r => r.FindConversationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Conversation?)null);

        var result = await _sut.SendMessageAsync(senderUserId, recipientProfile.Id, "E aí, bora jogar?");

        Assert.Equal(senderProfile.Id, result.Message.SenderProfileId);
        Assert.Equal("E aí, bora jogar?", result.Message.Content);
        Assert.Equal(senderProfile.ApplicationUserId, result.SenderUserId);
        Assert.Equal(recipientProfile.ApplicationUserId, result.RecipientUserId);
        _chatRepositoryMock.Verify(r => r.SaveNewMessageAsync(It.IsAny<Conversation>(), It.IsAny<Message>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ComConversaJaExistente_ReaproveitaAConversa()
    {
        var senderUserId = Guid.NewGuid();
        var senderProfile = CreateValidProfile(senderUserId, "Remetente");
        var recipientProfile = CreateValidProfile(Guid.NewGuid(), "Destinatario");
        var existingConversation = Conversation.Create(senderProfile.Id, recipientProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>())).ReturnsAsync(senderProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(recipientProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(recipientProfile);
        _chatRepositoryMock.Setup(r => r.FindConversationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingConversation);

        await _sut.SendMessageAsync(senderUserId, recipientProfile.Id, "De novo aqui");

        _chatRepositoryMock.Verify(
            r => r.SaveNewMessageAsync(
                It.Is<Conversation>(c => c.Id == existingConversation.Id),
                It.IsAny<Message>(),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ComRemetenteSemPerfil_LancaNotFoundException()
    {
        var senderUserId = Guid.NewGuid();
        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>())).ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SendMessageAsync(senderUserId, Guid.NewGuid(), "Oi"));
    }

    [Fact]
    public async Task SendMessageAsync_ComDestinatarioInexistente_LancaNotFoundException()
    {
        var senderUserId = Guid.NewGuid();
        var senderProfile = CreateValidProfile(senderUserId);
        var recipientProfileId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>())).ReturnsAsync(senderProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(recipientProfileId, It.IsAny<CancellationToken>())).ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SendMessageAsync(senderUserId, recipientProfileId, "Oi"));
    }

    [Fact]
    public async Task GetMessagesAsync_ComConversaQueNaoParticipa_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var conversationOfOtherPeople = Conversation.Create(Guid.NewGuid(), Guid.NewGuid());

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _chatRepositoryMock.Setup(r => r.GetConversationByIdAsync(conversationOfOtherPeople.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conversationOfOtherPeople);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetMessagesAsync(userId, conversationOfOtherPeople.Id, 1, 20));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(500, 50)]
    [InlineData(10, 10)]
    public async Task GetMessagesAsync_ClampaPageSize(int inputPageSize, int expectedPageSize)
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var conversation = Conversation.Create(profile.Id, Guid.NewGuid());

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _chatRepositoryMock.Setup(r => r.GetConversationByIdAsync(conversation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conversation);
        _chatRepositoryMock
            .Setup(r => r.GetMessagesAsync(conversation.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Message>(), 0));

        var result = await _sut.GetMessagesAsync(userId, conversation.Id, 1, inputPageSize);

        Assert.Equal(expectedPageSize, result.PageSize);
    }

    [Fact]
    public async Task MarkAsReadAsync_ComConversaQueNaoParticipa_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var conversationOfOtherPeople = Conversation.Create(Guid.NewGuid(), Guid.NewGuid());

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _chatRepositoryMock.Setup(r => r.GetConversationByIdAsync(conversationOfOtherPeople.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conversationOfOtherPeople);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.MarkAsReadAsync(userId, conversationOfOtherPeople.Id));
    }

    [Fact]
    public async Task GetConversationWithAsync_SemConversaExistente_RetornaNull()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _chatRepositoryMock
            .Setup(r => r.GetConversationsForProfileAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConversationSummaryDto>());

        var result = await _sut.GetConversationWithAsync(userId, Guid.NewGuid());

        Assert.Null(result);
    }
}
