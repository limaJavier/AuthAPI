using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.LoginWithEmail;

public record LoginWithEmailCommand(
    string Email,
    string Password
) : ICommand<Result<AuthResult>>;