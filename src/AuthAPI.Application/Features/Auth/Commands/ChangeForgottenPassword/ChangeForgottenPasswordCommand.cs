using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.ChangeForgottenPassword;

public record ChangeForgottenPasswordCommand(
    string VerificationToken,
    string Password
) : ICommand<Result>;