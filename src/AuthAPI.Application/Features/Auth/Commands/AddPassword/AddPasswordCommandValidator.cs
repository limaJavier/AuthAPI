using AuthAPI.Application.Common.Validation;
using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.AddPassword;

public class AddPasswordCommandValidator : AbstractValidator<AddPasswordCommand>
{
    public AddPasswordCommandValidator()
    {
        RuleFor(x => x.Password).Password();
    }
}
