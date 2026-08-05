using FluentValidation;

namespace GtaConnect.Application.Features.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Platform)
            .IsInEnum();

        RuleFor(x => x.GameTitle)
            .IsInEnum();
    }
}
