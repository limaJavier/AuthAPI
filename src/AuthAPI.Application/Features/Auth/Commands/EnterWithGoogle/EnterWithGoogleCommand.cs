using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.EnterWithGoogle;

public record EnterWithGoogleCommand(
    string GoogleToken
) : ICommand<Result<AuthResult>>;
