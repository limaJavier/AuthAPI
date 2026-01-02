using FluentValidation;

namespace AuthAPI.Application.Features.Auth.Commands.Refresh;

public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotNull()
            .MaximumLength(200);
    }
}