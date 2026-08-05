using GtaConnect.Application.Features.Auth;

namespace GtaConnect.Application.Tests.Features.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ComDadosValidos_NaoRetornaErros()
    {
        var request = new LoginRequestDto("jogador@exemplo.com", "qualquer-senha");

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ComEmailVazio_RetornaErro()
    {
        var request = new LoginRequestDto("", "qualquer-senha");

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequestDto.Email));
    }

    [Fact]
    public async Task Validate_ComSenhaVazia_RetornaErro()
    {
        var request = new LoginRequestDto("jogador@exemplo.com", "");

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequestDto.Password));
    }
}
