using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.PlayerSearch;

// DTO próprio da feature PlayerSearch (não reaproveita ProfileResponseDto) — mantém "ver
// outros jogadores" desacoplado de "meu perfil", mesmo com campos parecidos hoje.
// Nunca inclui email/ApplicationUserId — só dado já público na tela de perfil.
public record PlayerSummaryDto(
    Guid Id,
    string DisplayName,
    Platform Platform,
    GameTitle GameTitle,
    string? Bio,
    PlaystyleTag PlaystyleTags,
    AvailabilityTag AvailabilityTags,
    Region? Region,
    int HoursPlayed,
    string? FavoriteModes,
    string? AvatarPath,
    DateTime CreatedAtUtc,
    double? AverageRating,
    int RatingCount,
    bool IsOnline);
