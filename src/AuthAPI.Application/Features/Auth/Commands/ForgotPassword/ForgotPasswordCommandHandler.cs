using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Application.Features.Auth.Events.Verification;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IVerificationSessionManager verificationSessionManager,
    IApplicationEventQueue applicationEventQueue
) : ICommandHandler<ForgotPasswordCommand, Result<VerificationResult>>
{
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IApplicationEventQueue _applicationEventQueue = applicationEventQueue;

    public async ValueTask<Result<VerificationResult>> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(command.Email);
        if (user is null)
            return Error.NotFound($"User with email {command.Email} does not exist");

        if (user.PasswordHash is null) // Check user has a password
            return Error.Conflict($"User with email {command.Email} does not have a password");
        else if (!user.IsVerified) // Check user is verified
            return Error.Conflict($"User with email {command.Email} is not verified");

        // Create new verification session
        var verificationToken = await _verificationSessionManager.AddSessionAsync(command.Email);

        await _applicationEventQueue.PushAsync(new VerificationEvent(verificationToken, VerificationEventType.Password));

        return new VerificationResult(verificationToken);
    }
}