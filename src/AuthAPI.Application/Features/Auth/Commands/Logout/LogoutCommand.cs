using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(
    string RefreshToken
) : ICommand<Result>;