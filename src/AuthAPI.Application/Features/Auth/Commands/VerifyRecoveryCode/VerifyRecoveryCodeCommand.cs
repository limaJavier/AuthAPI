using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.VerifyRecoveryCode;

public record VerifyRecoveryCodeCommand(
    string VerificationToken,
    string VerificationCode
) : ICommand<Result>;