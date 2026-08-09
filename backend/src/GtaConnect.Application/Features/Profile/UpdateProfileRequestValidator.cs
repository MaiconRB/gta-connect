using FluentValidation;
using GtaConnect.Application.Resources;
using GtaConnect.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Application.Features.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage(localizer["Profile_BioTooLong"]);

        RuleFor(x => x.FavoriteModes)
            .MaximumLength(200).WithMessage(localizer["Profile_FavoriteModesTooLong"]);

        RuleFor(x => x.HoursPlayed)
            .InclusiveBetween(0, 100_000).WithMessage(localizer["Profile_HoursPlayedOutOfRange"]);

        RuleFor(x => x.PlaystyleTags)
            .Must(tags => (tags & ~PlaystyleTag.All) == 0)
            .WithMessage(localizer["Profile_PlaystyleTagsInvalid"]);
    }
}
