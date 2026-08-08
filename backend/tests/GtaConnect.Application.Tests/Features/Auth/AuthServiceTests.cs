using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Auth;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Auth;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _identityServiceMock.Object,
            _playerProfileRepositoryMock.Object,
            _jwtTokenGeneratorMock.Object,
            new RegisterRequestValidator(NoOpStringLocalizer.Create()),
            new LoginRequestValidator(),
            NoOpStringLocalizer.Create());
    }

    [Fact]
    public async Task RegisterAsync_ComDadosValidos_CriaPerfilERetornaToken()
    {
        var userId = Guid.NewGuid();
        var request = new RegisterRequestDto("jogador@exemplo.com", "Senha123", "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

        _identityServiceMock
            .Setup(s => s.CreateUserAsync(request.Email, request.Password))
            .ReturnsAsync(new CreateUserResult(true, userId, []));

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(userId, request.Email, request.DisplayName))
            .Returns(new JwtToken("fake-token", DateTime.UtcNow.AddHours(1)));

        var response = await _sut.RegisterAsync(request);

        Assert.Equal("fake-token", response.Token);
        Assert.Equal(request.Email, response.Email);
        Assert.Equal(request.DisplayName, response.DisplayName);
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

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(userId, request.Email, profile.DisplayName))
            .Returns(new JwtToken("fake-token", DateTime.UtcNow.AddHours(1)));

        var response = await _sut.LoginAsync(request);

        Assert.Equal("fake-token", response.Token);
        Assert.Equal(profile.DisplayName, response.DisplayName);
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
}
