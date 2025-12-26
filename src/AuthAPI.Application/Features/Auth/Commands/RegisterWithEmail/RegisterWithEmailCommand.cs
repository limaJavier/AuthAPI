using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.RegisterWithEmail;

public record RegisterWithEmailCommand(
    string Name,
    string Email,
    string Password
) : ICommand<Result<VerificationResult>>;