using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.PlayerSearch;

// Semântica dos bitmasks (PlaystyleTags/AvailabilityTags): None/0 = sem filtro (traz todo
// mundo); qualquer outro valor = "casa se o perfil tiver QUALQUER uma das tags marcadas".
public record PlayerSearchFilterDto(
    Platform? Platform,
    PlaystyleTag PlaystyleTags,
    Region? Region,
    AvailabilityTag AvailabilityTags,
    int Page,
    int PageSize);
