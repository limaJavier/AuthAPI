using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.Refresh;

public record RefreshCommand(
    string RefreshToken
) : ICommand<Result<AuthResult>>;