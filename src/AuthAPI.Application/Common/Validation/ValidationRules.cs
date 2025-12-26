using FluentValidation;

namespace AuthAPI.Application.Common.Validation;

public static class ValidationRules
{
    public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }

    public static IRuleBuilder<T, string> Email<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");
    }
}
