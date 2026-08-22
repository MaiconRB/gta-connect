using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Entities;

// Não guarda texto pronto — RelatedEntityId muda de significado por Type (ConversationId,
// ConnectionId, PostId, ou null pra RatingReceived) e o texto final é montado no frontend,
// via i18n, a partir do Type + nome do ator (resolvido pelo repositório na leitura).
public class Notification
{
    public Guid Id { get; private set; }

    public Guid RecipientProfileId { get; private set; }

    public Guid ActorProfileId { get; private set; }

    public NotificationType Type { get; private set; }

    public Guid? RelatedEntityId { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    private Notification()
    {
    }

    public static Notification Create(Guid recipientProfileId, Guid actorProfileId, NotificationType type, Guid? relatedEntityId)
    {
        if (recipientProfileId == Guid.Empty || actorProfileId == Guid.Empty)
        {
            throw new ArgumentException("Destinatário e autor da notificação não podem ser vazios.");
        }

        if (recipientProfileId == actorProfileId)
        {
            throw new ArgumentException("Não é possível notificar a si mesmo.");
        }

        return new Notification
        {
            Id = Guid.NewGuid(),
            RecipientProfileId = recipientProfileId,
            ActorProfileId = actorProfileId,
            Type = type,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = DateTime.UtcNow;
    }
}
