using GtaConnect.Application.Features.Profile;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Tests.Features.Profile;

public class UpdateProfileRequestValidatorTests
{
    private readonly UpdateProfileRequestValidator _validator = new(NoOpStringLocalizer.Create());

    [Fact]
    public async Task Validate_ComDadosValidos_NaoRetornaErros()
    {
        var request = new UpdateProfileRequestDto("Gosto de heists e RP.", PlaystyleTag.Corrida | PlaystyleTag.RolePlay, 100, "Cayo Perico");

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ComBioMuitoLonga_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(new string('a', 501), PlaystyleTag.None, 0, null);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.Bio));
    }

    [Fact]
    public async Task Validate_ComFavoriteModesMuitoLongo_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(null, PlaystyleTag.None, 0, new string('a', 201));

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.FavoriteModes));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100_001)]
    public async Task Validate_ComHorasJogadasForaDoIntervalo_RetornaErro(int hoursPlayed)
    {
        var request = new UpdateProfileRequestDto(null, PlaystyleTag.None, hoursPlayed, null);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.HoursPlayed));
    }

    [Fact]
    public async Task Validate_ComTagDeEstiloInvalida_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(null, (PlaystyleTag)(1 << 20), 0, null);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.PlaystyleTags));
    }
}
