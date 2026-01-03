using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.VerifyRecoveryCode;

public class VerifyRecoveryCodeCommandHandler(
    IVerificationSessionManager verificationSessionManager
) : ICommandHandler<VerifyRecoveryCodeCommand, Result>
{
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;

    public async ValueTask<Result> Handle(VerifyRecoveryCodeCommand command, CancellationToken cancellationToken)
    {
        // Get session
        var session = await _verificationSessionManager.GetSessionAsync(command.VerificationToken);
        if (session is null)
            return Error.Unauthorized($"Verification-Session with token {command.VerificationToken} does not exist");

        var result = session.Verify(command.VerificationCode); // Verify code
        if (result.IsFailure)
            return result.Error;

        await _verificationSessionManager.UpdateSessionAsync(session); // Update session

        return result;
    }
}