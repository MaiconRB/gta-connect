using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Tests.Entities;

public class NotificationTests
{
    [Fact]
    public void Create_ComDadosValidos_CriaComoNaoLida()
    {
        var recipientProfileId = Guid.NewGuid();
        var actorProfileId = Guid.NewGuid();
        var relatedEntityId = Guid.NewGuid();

        var notification = Notification.Create(recipientProfileId, actorProfileId, NotificationType.MessageReceived, relatedEntityId);

        Assert.Equal(recipientProfileId, notification.RecipientProfileId);
        Assert.Equal(actorProfileId, notification.ActorProfileId);
        Assert.Equal(NotificationType.MessageReceived, notification.Type);
        Assert.Equal(relatedEntityId, notification.RelatedEntityId);
        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAtUtc);
    }

    [Fact]
    public void Create_SemRelatedEntityId_AceitaNull()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), NotificationType.RatingReceived, null);

        Assert.Null(notification.RelatedEntityId);
    }

    [Fact]
    public void Create_ComMesmoPerfilComoDestinatarioEAutor_LancaArgumentException()
    {
        var profileId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Notification.Create(profileId, profileId, NotificationType.PostLiked, null));
    }

    [Fact]
    public void MarkAsRead_MudaParaLidaEPreencheReadAtUtc()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), NotificationType.ConnectionRequestReceived, Guid.NewGuid());

        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAtUtc);
    }

    [Fact]
    public void MarkAsRead_ChamadoDuasVezes_NaoLancaErro()
    {
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), NotificationType.ConnectionRequestReceived, Guid.NewGuid());

        notification.MarkAsRead();
        var firstReadAt = notification.ReadAtUtc;
        notification.MarkAsRead();

        Assert.Equal(firstReadAt, notification.ReadAtUtc);
    }
}
