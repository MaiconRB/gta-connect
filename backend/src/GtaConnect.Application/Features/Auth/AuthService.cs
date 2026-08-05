using FluentValidation;
using GtaConnect.Application.Common.Extensions;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public AuthService(
        IIdentityService identityService,
        IPlayerProfileRepository playerProfileRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator)
    {
        _identityService = identityService;
        _playerProfileRepository = playerProfileRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAppExceptionAsync(request, cancellationToken);

        var createResult = await _identityService.CreateUserAsync(request.Email, request.Password);
        if (!createResult.Succeeded)
        {
            throw new ValidationAppException("email", string.Join(" ", createResult.Errors));
        }

        var profile = PlayerProfile.Create(createResult.UserId!.Value, request.DisplayName, request.Platform, request.GameTitle);
        await _playerProfileRepository.AddAsync(profile, cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(createResult.UserId.Value, request.Email, profile.DisplayName);
        return new AuthResponseDto(token.Value, token.ExpiresAtUtc, request.Email, profile.DisplayName);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAppExceptionAsync(request, cancellationToken);

        var userId = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
        if (userId is null)
        {
            throw new ValidationAppException("credentials", "Email ou senha inválidos.");
        }

        var profile = await _playerProfileRepository.GetByUserIdAsync(userId.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId.Value);

        var token = _jwtTokenGenerator.GenerateToken(userId.Value, request.Email, profile.DisplayName);
        return new AuthResponseDto(token.Value, token.ExpiresAtUtc, request.Email, profile.DisplayName);
    }
}
