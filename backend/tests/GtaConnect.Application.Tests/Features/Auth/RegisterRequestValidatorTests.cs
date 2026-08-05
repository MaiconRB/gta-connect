using GtaConnect.Application.Features.Auth;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Tests.Features.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ComDadosValidos_NaoRetornaErros()
    {
        var request = new RegisterRequestDto("jogador@exemplo.com", "Senha123", "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-um-email")]
    public async Task Validate_ComEmailInvalido_RetornaErro(string email)
    {
        var request = new RegisterRequestDto(email, "Senha123", "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequestDto.Email));
    }

    [Theory]
    [InlineData("curta1")]
    [InlineData("semmaiuscula1")]
    [InlineData("SEMMINUSCULA1")]
    [InlineData("SemNumero")]
    public async Task Validate_ComSenhaFraca_RetornaErro(string password)
    {
        var request = new RegisterRequestDto("jogador@exemplo.com", password, "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequestDto.Password));
    }

    [Fact]
    public async Task Validate_ComDisplayNameVazio_RetornaErro()
    {
        var request = new RegisterRequestDto("jogador@exemplo.com", "Senha123", "", Platform.Ps5, GameTitle.GtaV);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequestDto.DisplayName));
    }
}
