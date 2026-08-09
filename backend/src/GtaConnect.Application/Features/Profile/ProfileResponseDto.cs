using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Profile;

// AvatarPath vem como caminho relativo ("/uploads/avatars/xxx.jpg") ou null — a Api é
// quem monta a URL absoluta antes de devolver ao frontend (ver ProfileController).
// A Application não conhece HttpContext/scheme/host.
public record ProfileResponseDto(
    Guid Id,
    string DisplayName,
    Platform Platform,
    GameTitle GameTitle,
    string? Bio,
    PlaystyleTag PlaystyleTags,
    int HoursPlayed,
    string? FavoriteModes,
    string? AvatarPath,
    DateTime CreatedAtUtc);
