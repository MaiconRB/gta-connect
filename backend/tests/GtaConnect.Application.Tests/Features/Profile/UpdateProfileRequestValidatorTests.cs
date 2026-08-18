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
        var request = new UpdateProfileRequestDto("Gosto de heists e RP.", PlaystyleTag.Corrida | PlaystyleTag.RolePlay, 100, "Cayo Perico", Region.Sudeste, AvailabilityTag.Noite);

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ComBioMuitoLonga_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(new string('a', 501), PlaystyleTag.None, 0, null, null, AvailabilityTag.None);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.Bio));
    }

    [Fact]
    public async Task Validate_ComFavoriteModesMuitoLongo_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(null, PlaystyleTag.None, 0, new string('a', 201), null, AvailabilityTag.None);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.FavoriteModes));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100_001)]
    public async Task Validate_ComHorasJogadasForaDoIntervalo_RetornaErro(int hoursPlayed)
    {
        var request = new UpdateProfileRequestDto(null, PlaystyleTag.None, hoursPlayed, null, null, AvailabilityTag.None);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.HoursPlayed));
    }

    [Fact]
    public async Task Validate_ComTagDeEstiloInvalida_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(null, (PlaystyleTag)(1 << 20), 0, null, null, AvailabilityTag.None);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.PlaystyleTags));
    }

    [Fact]
    public async Task Validate_ComTagDeDisponibilidadeInvalida_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(null, PlaystyleTag.None, 0, null, null, (AvailabilityTag)(1 << 20));

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.AvailabilityTags));
    }

    [Fact]
    public async Task Validate_ComRegiaoInvalida_RetornaErro()
    {
        var request = new UpdateProfileRequestDto(null, PlaystyleTag.None, 0, null, (Region)999, AvailabilityTag.None);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequestDto.Region));
    }
}
