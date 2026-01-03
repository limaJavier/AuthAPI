using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.VerifyRecoveryCode;

public class VerifyRecoveryCodeCommandValidator : AbstractValidator<VerifyRecoveryCodeCommand>
{
    public VerifyRecoveryCodeCommandValidator()
    {
        RuleFor(x => x.VerificationToken)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(x => x.VerificationCode)
            .NotEmpty()
            .MaximumLength(100);
    }
}