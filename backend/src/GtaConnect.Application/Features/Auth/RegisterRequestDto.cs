using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Auth;

public record RegisterRequestDto(
    string Email,
    string Password,
    string DisplayName,
    Platform Platform,
    GameTitle GameTitle);
