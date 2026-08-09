using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Entities;

public class PlayerProfile
{
    private const int MaxBioLength = 500;
    private const int MaxFavoriteModesLength = 200;
    private const int MaxHoursPlayed = 100_000;

    public Guid Id { get; private set; }

    /// <summary>
    /// Chave estrangeira lógica para o ApplicationUser (Identity), que vive na camada
    /// de Infrastructure. O Domain não conhece o Identity — só guarda o Id de referência.
    /// </summary>
    public Guid ApplicationUserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public Platform Platform { get; private set; }

    public GameTitle GameTitle { get; private set; }

    public string? Bio { get; private set; }

    public PlaystyleTag PlaystyleTags { get; private set; } = PlaystyleTag.None;

    public int HoursPlayed { get; private set; }

    public string? FavoriteModes { get; private set; }

    public string? AvatarPath { get; private set; }

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

    /// <summary>
    /// Atualiza os campos editáveis do perfil (bio, estilo de jogo, horas jogadas, modos
    /// favoritos). Não mexe em DisplayName/Platform/GameTitle/AvatarPath — cada um tem seu
    /// próprio motivo pra ficar fora daqui (ver PROJETO.md e o método SetAvatar).
    /// </summary>
    public void UpdateProfile(string? bio, PlaystyleTag playstyleTags, int hoursPlayed, string? favoriteModes)
    {
        var trimmedBio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        if (trimmedBio is { Length: > MaxBioLength })
        {
            throw new ArgumentException($"Bio não pode ter mais de {MaxBioLength} caracteres.", nameof(bio));
        }

        // Enum.IsDefined não serve pra [Flags] combinado (falha pra combinações válidas de
        // múltiplas flags) — a checagem certa é bitwise: nenhum bit fora do conjunto conhecido.
        if ((playstyleTags & ~PlaystyleTag.All) != 0)
        {
            throw new ArgumentException("Uma ou mais tags de estilo de jogo são inválidas.", nameof(playstyleTags));
        }

        if (hoursPlayed < 0 || hoursPlayed > MaxHoursPlayed)
        {
            throw new ArgumentOutOfRangeException(nameof(hoursPlayed), $"Horas jogadas deve estar entre 0 e {MaxHoursPlayed}.");
        }

        var trimmedFavoriteModes = string.IsNullOrWhiteSpace(favoriteModes) ? null : favoriteModes.Trim();
        if (trimmedFavoriteModes is { Length: > MaxFavoriteModesLength })
        {
            throw new ArgumentException($"Modos favoritos não pode ter mais de {MaxFavoriteModesLength} caracteres.", nameof(favoriteModes));
        }

        Bio = trimmedBio;
        PlaystyleTags = playstyleTags;
        HoursPlayed = hoursPlayed;
        FavoriteModes = trimmedFavoriteModes;
    }

    /// <summary>
    /// Método separado de UpdateProfile de propósito: upload de foto é uma ação
    /// independente da edição dos outros campos (falhar um não deve invalidar o outro).
    /// </summary>
    public void SetAvatar(string avatarPath)
    {
        if (string.IsNullOrWhiteSpace(avatarPath))
        {
            throw new ArgumentException("AvatarPath não pode ser vazio.", nameof(avatarPath));
        }

        AvatarPath = avatarPath;
    }
}
