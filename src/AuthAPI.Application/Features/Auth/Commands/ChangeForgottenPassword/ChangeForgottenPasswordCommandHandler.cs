using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.ChangeForgottenPassword;

public class ChangeForgottenPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IVerificationSessionManager verificationSessionManager,
    IHasher hasher
) : ICommandHandler<ChangeForgottenPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result> Handle(ChangeForgottenPasswordCommand command, CancellationToken cancellationToken)
    {
        // Get session
        var session = await _verificationSessionManager.GetSessionAsync(command.VerificationToken);
        if (session is null)
            return Error.Unauthorized($"Verification-Session with token {command.VerificationToken} does not exist");

        // Check session is verified
        if (!session.IsVerified)
            return Error.Conflict($"Verification-Session with token {command.VerificationToken} is not verified");

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(session.Email);
        if (user is null)
            return Error.Conflict($"User with email {session.Email} does not exist");

        // Change user's password
        user.ChangePassword(command.Password, _hasher);
        user.RevokeAllRefreshTokens();

        await _verificationSessionManager.RemoveSessionAsync(session.Token); // Remove verification session

        await _unitOfWork.CommitAsync();

        return Result.Success();
    }
}