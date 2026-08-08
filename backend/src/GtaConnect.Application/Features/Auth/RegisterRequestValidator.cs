using FluentValidation;
using GtaConnect.Application.Resources;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Application.Features.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage(localizer["Password_RequiresUppercase"])
            .Matches("[a-z]").WithMessage(localizer["Password_RequiresLowercase"])
            .Matches("[0-9]").WithMessage(localizer["Password_RequiresDigit"]);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Platform)
            .IsInEnum();

        RuleFor(x => x.GameTitle)
            .IsInEnum();
    }
}
