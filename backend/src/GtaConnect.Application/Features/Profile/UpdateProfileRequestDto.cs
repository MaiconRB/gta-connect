using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Profile;

public record UpdateProfileRequestDto(
    string? Bio,
    PlaystyleTag PlaystyleTags,
    int HoursPlayed,
    string? FavoriteModes,
    Region? Region,
    AvailabilityTag AvailabilityTags);
