using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Entities;

public class PlayerProfile
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Chave estrangeira lógica para o ApplicationUser (Identity), que vive na camada
    /// de Infrastructure. O Domain não conhece o Identity — só guarda o Id de referência.
    /// </summary>
    public Guid ApplicationUserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public Platform Platform { get; private set; }

    public GameTitle GameTitle { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    // Construtor privado: o EF Core materializa entidades sem passar pelas regras de
    // negócio do "Create" abaixo. Fora do EF Core, a única forma de criar um PlayerProfile
    // válido é pelo factory method.
    private PlayerProfile()
    {
    }

    public static PlayerProfile Create(Guid applicationUserId, string displayName, Platform platform, GameTitle gameTitle)
    {
        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException("ApplicationUserId não pode ser vazio.", nameof(applicationUserId));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("DisplayName não pode ser vazio.", nameof(displayName));
        }

        return new PlayerProfile
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = applicationUserId,
            DisplayName = displayName.Trim(),
            Platform = platform,
            GameTitle = gameTitle,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
