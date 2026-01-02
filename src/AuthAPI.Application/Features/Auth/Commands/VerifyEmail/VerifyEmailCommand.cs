using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(
    string VerificationToken,
    string VerificationCode
) : ICommand<Result<AuthResult>>;