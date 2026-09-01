namespace GtaConnect.Domain.Entities;

// Uma GameSession pertence a uma Connection (o par) — não modela sessões em grupo de
// propósito: o app inteiro já é organizado em torno de conexões pareadas (Connection,
// Rating antigo), então "registrar que jogamos junto" fica escopado ao mesmo par, sem
// abrir um modelo N-a-N de participantes que nada mais no app usa ainda.
public class GameSession
{
    public Guid Id { get; private set; }

    public Guid ConnectionId { get; private set; }

    public Guid LoggedByProfileId { get; private set; }

    public DateTime PlayedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private GameSession()
    {
    }

    public static GameSession Create(Guid connectionId, Guid loggedByProfileId, DateTime? playedAtUtc = null)
    {
        if (connectionId == Guid.Empty || loggedByProfileId == Guid.Empty)
        {
            throw new ArgumentException("Conexão e perfil de quem registrou não podem ser vazios.");
        }

        var now = DateTime.UtcNow;
        var resolvedPlayedAt = playedAtUtc ?? now;

        if (resolvedPlayedAt > now.AddMinutes(5))
        {
            // Pequena folga (5 min) pra diferença de relógio entre cliente/servidor —
            // mas não dá pra registrar uma sessão "no futuro" de propósito.
            throw new ArgumentException("A data da sessão não pode estar no futuro.", nameof(playedAtUtc));
        }

        return new GameSession
        {
            Id = Guid.NewGuid(),
            ConnectionId = connectionId,
            LoggedByProfileId = loggedByProfileId,
            PlayedAtUtc = resolvedPlayedAt,
            CreatedAtUtc = now,
        };
    }
}
