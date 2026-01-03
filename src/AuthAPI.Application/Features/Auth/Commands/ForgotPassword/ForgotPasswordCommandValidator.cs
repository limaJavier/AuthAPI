using AuthAPI.Application.Common.Validation;
using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).Email();
    }
}