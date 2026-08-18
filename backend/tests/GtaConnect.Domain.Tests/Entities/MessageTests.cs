using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class MessageTests
{
    [Fact]
    public void Create_ComConteudoValido_CriaMensagemComConteudoAparado()
    {
        var conversationId = Guid.NewGuid();
        var senderProfileId = Guid.NewGuid();

        var message = Message.Create(conversationId, senderProfileId, "  bora jogar heist?  ");

        Assert.Equal(conversationId, message.ConversationId);
        Assert.Equal(senderProfileId, message.SenderProfileId);
        Assert.Equal("bora jogar heist?", message.Content);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ComConteudoVazio_LancaArgumentException(string? content)
    {
        Assert.Throws<ArgumentException>(() => Message.Create(Guid.NewGuid(), Guid.NewGuid(), content!));
    }

    [Fact]
    public void Create_ComConteudoMuitoLongo_LancaArgumentException()
    {
        var tooLong = new string('a', 2001);

        Assert.Throws<ArgumentException>(() => Message.Create(Guid.NewGuid(), Guid.NewGuid(), tooLong));
    }
}
