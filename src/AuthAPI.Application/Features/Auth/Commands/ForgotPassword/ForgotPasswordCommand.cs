using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(
    string Email
) : ICommand<Result<VerificationResult>>;