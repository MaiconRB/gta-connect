namespace GtaConnect.Application.Features.Auth;

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAtUtc,
    string Email,
    string DisplayName);
