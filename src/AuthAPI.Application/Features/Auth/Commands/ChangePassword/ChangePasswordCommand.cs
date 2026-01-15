using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    string OldPassword,
    string NewPassword
) : ICommand<Result>;
