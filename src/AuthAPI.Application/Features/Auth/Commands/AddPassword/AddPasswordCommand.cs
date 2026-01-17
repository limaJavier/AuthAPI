using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.AddPassword;

public record AddPasswordCommand(
    string Password
) : ICommand<Result>;
