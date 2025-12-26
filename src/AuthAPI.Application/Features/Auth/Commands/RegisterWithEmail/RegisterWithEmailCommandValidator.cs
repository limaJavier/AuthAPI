using AuthAPI.Application.Common.Validation;
using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.RegisterWithEmail;

public class RegisterWithEmailCommandValidator : AbstractValidator<RegisterWithEmailCommand>
{
    public RegisterWithEmailCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).Email();
        RuleFor(x => x.Password).Password();
    }
}