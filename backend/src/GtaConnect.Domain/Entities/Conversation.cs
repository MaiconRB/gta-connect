namespace GtaConnect.Domain.Entities;

// Só existe conversa 1:1 (mensagens diretas) — não é chat em grupo. ParticipantAId é
// sempre o menor Guid dos dois: normaliza a ordem na criação pra permitir um índice
// único no par de participantes, independente de quem inicia a conversa.
public class Conversation
{
    public Guid Id { get; private set; }

    public Guid ParticipantAId { get; private set; }

    public Guid ParticipantBId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime LastMessageAtUtc { get; private set; }

    public DateTime? ParticipantALastReadAtUtc { get; private set; }

    public DateTime? ParticipantBLastReadAtUtc { get; private set; }

    private Conversation()
    {
    }

    public static Conversation Create(Guid participantXId, Guid participantYId)
    {
        if (participantXId == Guid.Empty || participantYId == Guid.Empty)
        {
            throw new ArgumentException("Os participantes da conversa não podem ser vazios.");
        }

        if (participantXId == participantYId)
        {
            throw new ArgumentException("Não é possível iniciar uma conversa consigo mesmo.");
        }

        var (participantAId, participantBId) = NormalizeParticipantOrder(participantXId, participantYId);

        var now = DateTime.UtcNow;
        return new Conversation
        {
            Id = Guid.NewGuid(),
            ParticipantAId = participantAId,
            ParticipantBId = participantBId,
            CreatedAtUtc = now,
            LastMessageAtUtc = now,
        };
    }

    /// <summary>
    /// Cria a mensagem e atualiza LastMessageAtUtc juntos — quem chama nunca esquece de
    /// "tocar" a conversa ao mandar uma mensagem, porque os dois acontecem numa chamada só.
    /// </summary>
    public Message PostMessage(Guid senderProfileId, string content)
    {
        EnsureParticipant(senderProfileId);

        var message = Message.Create(Id, senderProfileId, content);
        LastMessageAtUtc = message.SentAtUtc;
        return message;
    }

    public void MarkReadBy(Guid readerProfileId, DateTime readAtUtc)
    {
        EnsureParticipant(readerProfileId);

        if (readerProfileId == ParticipantAId)
        {
            ParticipantALastReadAtUtc = readAtUtc;
        }
        else
        {
            ParticipantBLastReadAtUtc = readAtUtc;
        }
    }

    public Guid GetOtherParticipantId(Guid profileId)
    {
        EnsureParticipant(profileId);
        return profileId == ParticipantAId ? ParticipantBId : ParticipantAId;
    }

    public bool HasParticipant(Guid profileId) => profileId == ParticipantAId || profileId == ParticipantBId;

    /// <summary>
    /// Mesma regra de ordenação usada em Create (menor Guid primeiro) — exposta pra quem
    /// precisa procurar uma conversa existente pelo par de participantes ANTES de decidir
    /// se cria uma nova (ver ChatService.SendMessageAsync).
    /// </summary>
    public static (Guid ParticipantAId, Guid ParticipantBId) NormalizeParticipantOrder(Guid participantXId, Guid participantYId) =>
        participantXId.CompareTo(participantYId) < 0
            ? (participantXId, participantYId)
            : (participantYId, participantXId);

    private void EnsureParticipant(Guid profileId)
    {
        if (!HasParticipant(profileId))
        {
            throw new ArgumentException("O perfil informado não participa desta conversa.", nameof(profileId));
        }
    }
}
