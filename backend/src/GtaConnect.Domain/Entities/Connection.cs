using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Entities;

// RequesterProfileId/AddresseeProfileId NÃO são normalizados por ordem (diferente de
// Conversation/Block) — quem pediu importa pra UI ("pedidos que enviei" vs "que recebi"),
// perder essa distinção quebraria a tela de gerenciamento.
public class Connection
{
    public Guid Id { get; private set; }

    public Guid RequesterProfileId { get; private set; }

    public Guid AddresseeProfileId { get; private set; }

    public ConnectionStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? RespondedAtUtc { get; private set; }

    private Connection()
    {
    }

    public static Connection Create(Guid requesterProfileId, Guid addresseeProfileId)
    {
        if (requesterProfileId == Guid.Empty || addresseeProfileId == Guid.Empty)
        {
            throw new ArgumentException("Os participantes da conexão não podem ser vazios.");
        }

        if (requesterProfileId == addresseeProfileId)
        {
            throw new ArgumentException("Não é possível se conectar consigo mesmo.");
        }

        return new Connection
        {
            Id = Guid.NewGuid(),
            RequesterProfileId = requesterProfileId,
            AddresseeProfileId = addresseeProfileId,
            Status = ConnectionStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>Só o endereçado (quem recebeu o pedido) pode aceitar.</summary>
    public void Accept(Guid respondingProfileId)
    {
        if (respondingProfileId != AddresseeProfileId)
        {
            throw new ArgumentException("Só quem recebeu o pedido pode aceitá-lo.", nameof(respondingProfileId));
        }

        EnsurePending();
        Status = ConnectionStatus.Accepted;
        RespondedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Qualquer um dos dois pode declinar — o endereçado recusando o pedido, ou o próprio
    /// solicitante cancelando o que enviou. Mesmo efeito nos dois casos: não vira conexão.
    /// </summary>
    public void Decline(Guid respondingProfileId)
    {
        if (!HasParticipant(respondingProfileId))
        {
            throw new ArgumentException("O perfil informado não participa desta conexão.", nameof(respondingProfileId));
        }

        EnsurePending();
        Status = ConnectionStatus.Declined;
        RespondedAtUtc = DateTime.UtcNow;
    }

    public bool HasParticipant(Guid profileId) => profileId == RequesterProfileId || profileId == AddresseeProfileId;

    public Guid GetOtherParticipantId(Guid profileId)
    {
        if (!HasParticipant(profileId))
        {
            throw new ArgumentException("O perfil informado não participa desta conexão.", nameof(profileId));
        }

        return profileId == RequesterProfileId ? AddresseeProfileId : RequesterProfileId;
    }

    private void EnsurePending()
    {
        if (Status != ConnectionStatus.Pending)
        {
            throw new ArgumentException("Só é possível responder um pedido de conexão pendente.");
        }
    }
}
