using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class ConversationTests
{
    [Fact]
    public void Create_NormalizaOrdemDosParticipantesPeloMenorGuid()
    {
        var smaller = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var larger = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var conversationStartedByLarger = Conversation.Create(larger, smaller);

        Assert.Equal(smaller, conversationStartedByLarger.ParticipantAId);
        Assert.Equal(larger, conversationStartedByLarger.ParticipantBId);
    }

    [Fact]
    public void Create_ComMesmoParticipanteDuasVezes_LancaArgumentException()
    {
        var participantId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Conversation.Create(participantId, participantId));
    }

    [Fact]
    public void Create_ComParticipanteVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Conversation.Create(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void PostMessage_ComParticipanteValido_CriaMensagemEAtualizaLastMessageAtUtc()
    {
        var participantA = Guid.NewGuid();
        var participantB = Guid.NewGuid();
        var conversation = Conversation.Create(participantA, participantB);
        var beforePost = conversation.LastMessageAtUtc;

        var message = conversation.PostMessage(participantA, "E aí, bora jogar?");

        Assert.Equal(conversation.Id, message.ConversationId);
        Assert.Equal(participantA, message.SenderProfileId);
        Assert.Equal("E aí, bora jogar?", message.Content);
        Assert.True(conversation.LastMessageAtUtc >= beforePost);
        Assert.Equal(message.SentAtUtc, conversation.LastMessageAtUtc);
    }

    [Fact]
    public void PostMessage_ComRemetenteQueNaoParticipa_LancaArgumentException()
    {
        var conversation = Conversation.Create(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => conversation.PostMessage(Guid.NewGuid(), "Oi"));
    }

    [Fact]
    public void MarkReadBy_ComParticipanteA_AtualizaSomenteParticipantALastReadAtUtc()
    {
        var participantA = Guid.NewGuid();
        var participantB = Guid.NewGuid();
        var conversation = Conversation.Create(participantA, participantB);
        var readAt = DateTime.UtcNow;

        // Create normaliza a ordem por menor Guid — marca como lido por quem efetivamente
        // virou ParticipantAId na conversa, não pela variável local "participantA".
        conversation.MarkReadBy(conversation.ParticipantAId, readAt);

        Assert.Equal(readAt, conversation.ParticipantALastReadAtUtc);
        Assert.Null(conversation.ParticipantBLastReadAtUtc);
    }

    [Fact]
    public void MarkReadBy_ComQuemNaoParticipa_LancaArgumentException()
    {
        var conversation = Conversation.Create(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => conversation.MarkReadBy(Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void GetOtherParticipantId_RetornaOOutroParticipante()
    {
        var participantA = Guid.NewGuid();
        var participantB = Guid.NewGuid();
        var conversation = Conversation.Create(participantA, participantB);

        Assert.Equal(conversation.ParticipantBId, conversation.GetOtherParticipantId(conversation.ParticipantAId));
        Assert.Equal(conversation.ParticipantAId, conversation.GetOtherParticipantId(conversation.ParticipantBId));
    }

    [Fact]
    public void GetOtherParticipantId_ComQuemNaoParticipa_LancaArgumentException()
    {
        var conversation = Conversation.Create(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => conversation.GetOtherParticipantId(Guid.NewGuid()));
    }

    [Fact]
    public void NormalizeParticipantOrder_RetornaMesmoResultadoIndependenteDaOrdemDeEntrada()
    {
        var smaller = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var larger = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var fromSmallerFirst = Conversation.NormalizeParticipantOrder(smaller, larger);
        var fromLargerFirst = Conversation.NormalizeParticipantOrder(larger, smaller);

        Assert.Equal((smaller, larger), fromSmallerFirst);
        Assert.Equal((smaller, larger), fromLargerFirst);
    }
}
