using GtaConnect.Application.Common;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Auth;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;

namespace GtaConnect.Application.Tests.Features.Auth;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var authOptions = Options.Create(new AuthOptions { FrontendUrl = "http://localhost:4200" });

        _sut = new AuthService(
            _identityServiceMock.Object,
            _playerProfileRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object,
            _emailServiceMock.Object,
            new RegisterRequestValidator(NoOpStringLocalizer.Create()),
            new LoginRequestValidator(),
            NoOpStringLocalizer.Create(),
            authOptions);
    }

    [Fact]
    public async Task RegisterAsync_ComDadosValidos_CriaPerfilERetornaToken()
    {
        var userId = Guid.NewGuid();
        var request = new RegisterRequestDto("jogador@exemplo.com", "Senha123", "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

        _identityServiceMock
            .Setup(s => s.CreateUserAsync(request.Email, request.Password))
            .ReturnsAsync(new CreateUserResult(true, userId, []));

        _identityServiceMock
            .Setup(s => s.GenerateEmailConfirmationTokenAsync(userId))
            .ReturnsAsync("fake-encoded-token");

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(userId, request.Email, request.DisplayName))
            .Returns(new JwtToken("fake-token", DateTime.UtcNow.AddHours(1)));

        var response = await _sut.RegisterAsync(request);

        Assert.Equal("fake-token", response.Token);
        Assert.Equal(request.Email, response.Email);
        Assert.Equal(request.DisplayName, response.DisplayName);
        Assert.False(response.EmailConfirmed);
        _playerProfileRepositoryMock.Verify(
            r => r.AddAsync(It.Is<PlayerProfile>(p => p.ApplicationUserId == userId && p.DisplayName == request.DisplayName), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ComDadosInvalidos_LancaValidationAppExceptionSemChamarIdentity()
    {
        var request = new RegisterRequestDto("email-invalido", "curta", "", Platform.Ps5, GameTitle.GtaV);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.RegisterAsync(request));

        _identityServiceMock.Verify(s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_QuandoIdentityFalha_LancaValidationAppException()
    {
        var request = new RegisterRequestDto("jogador@exemplo.com", "Senha123", "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

        _identityServiceMock
            .Setup(s => s.CreateUserAsync(request.Email, request.Password))
            .ReturnsAsync(new CreateUserResult(false, null, ["Email já está em uso."]));

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.RegisterAsync(request));

        _playerProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PlayerProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_RetornaToken()
    {
        var userId = Guid.NewGuid();
        var request = new LoginRequestDto("jogador@exemplo.com", "Senha123");
        var profile = PlayerProfile.Create(userId, "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

        _identityServiceMock
            .Setup(s => s.ValidateCredentialsAsync(request.Email, request.Password))
            .ReturnsAsync(userId);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _identityServiceMock
            .Setup(s => s.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(true);

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(userId, request.Email, profile.DisplayName))
            .Returns(new JwtToken("fake-token", DateTime.UtcNow.AddHours(1)));

        var response = await _sut.LoginAsync(request);

        Assert.Equal("fake-token", response.Token);
        Assert.Equal(profile.DisplayName, response.DisplayName);
        Assert.True(response.EmailConfirmed);
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisInvalidas_LancaValidationAppException()
    {
        var request = new LoginRequestDto("jogador@exemplo.com", "senha-errada");

        _identityServiceMock
            .Setup(s => s.ValidateCredentialsAsync(request.Email, request.Password))
            .ReturnsAsync((Guid?)null);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_QuandoPerfilNaoExiste_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var request = new LoginRequestDto("jogador@exemplo.com", "Senha123");

        _identityServiceMock
            .Setup(s => s.ValidateCredentialsAsync(request.Email, request.Password))
            .ReturnsAsync(userId);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task ConfirmEmailAsync_ComTokenValido_NaoLancaExcecao()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock
            .Setup(s => s.ConfirmEmailAsync(userId, "token-valido"))
            .ReturnsAsync(true);

        await _sut.ConfirmEmailAsync(userId, "token-valido");
    }

    [Fact]
    public async Task ConfirmEmailAsync_ComTokenInvalido_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock
            .Setup(s => s.ConfirmEmailAsync(userId, "token-invalido"))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.ConfirmEmailAsync(userId, "token-invalido"));
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_QuandoJaConfirmado_NaoBuscaEmailNemEnvia()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock
            .Setup(s => s.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(true);

        await _sut.ResendConfirmationEmailAsync(userId);

        _identityServiceMock.Verify(s => s.GetUserEmailAsync(It.IsAny<Guid>()), Times.Never);
        _emailServiceMock.Verify(
            s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_QuandoNaoConfirmado_EnviaNovoEmailDeConfirmacao()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock
            .Setup(s => s.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(s => s.GetUserEmailAsync(userId))
            .ReturnsAsync("jogador@exemplo.com");

        _identityServiceMock
            .Setup(s => s.GenerateEmailConfirmationTokenAsync(userId))
            .ReturnsAsync("fake-encoded-token");

        await _sut.ResendConfirmationEmailAsync(userId);

        _emailServiceMock.Verify(
            s => s.SendEmailAsync("jogador@exemplo.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_QuandoUsuarioNaoExiste_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();

        _identityServiceMock
            .Setup(s => s.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(false);

        _identityServiceMock
            .Setup(s => s.GetUserEmailAsync(userId))
            .ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ResendConfirmationEmailAsync(userId));
    }
}
