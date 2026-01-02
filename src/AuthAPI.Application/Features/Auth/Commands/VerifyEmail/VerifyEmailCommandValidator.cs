using AuthAPI.Application.Common.Validation;
using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.VerificationToken)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(x => x.VerificationCode)
            .NotEmpty()
            .MaximumLength(100);
    }
}