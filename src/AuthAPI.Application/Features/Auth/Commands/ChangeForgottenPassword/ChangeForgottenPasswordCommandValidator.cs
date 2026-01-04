using AuthAPI.Application.Common.Validation;
using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.ChangeForgottenPassword;

public class ChangeForgottenPasswordCommandValidator : AbstractValidator<ChangeForgottenPasswordCommand>
{
    public ChangeForgottenPasswordCommandValidator()
    {
        RuleFor(x => x.VerificationToken)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(x => x.Password).Password();
    }
}