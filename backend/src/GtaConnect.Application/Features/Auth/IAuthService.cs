namespace GtaConnect.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default);

    Task ResendConfirmationEmailAsync(Guid userId, CancellationToken cancellationToken = default);
}
