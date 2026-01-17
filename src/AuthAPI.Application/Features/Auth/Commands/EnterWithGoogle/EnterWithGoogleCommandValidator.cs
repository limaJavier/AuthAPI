using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.EnterWithGoogle;

public class EnterWithGoogleCommandValidator : AbstractValidator<EnterWithGoogleCommand>
{
    public EnterWithGoogleCommandValidator()
    {
        RuleFor(x => x.GoogleToken)
            .NotEmpty()
            .MaximumLength(3000);
    }
}
