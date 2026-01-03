using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IVerificationSessionManager verificationSessionManager,
    ITokenGenerator tokenGenerator,
    IHasher hasher
) : ICommandHandler<VerifyEmailCommand, Result<AuthResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result<AuthResult>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        //** Verify session
        var session = await _verificationSessionManager.GetSessionAsync(command.VerificationToken);

        if (session is null)
            return Error.Unauthorized($"Verification-Session with token {command.VerificationToken} does not exist");

        var verificationResult = session.Verify(command.VerificationCode);

        if (verificationResult.IsFailure)
            return verificationResult.Error;

        //** Verify user and generate tokens
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(session.Email);

        if (user is null)
            return Error.Conflict($"User with email {session.Email} was not found");

        user.Verify(); // Verify user
        var refreshToken = user.AddRefreshToken(_tokenGenerator, _hasher); // Generate refresh-token

        // Generate JWT token
        var accessToken = _tokenGenerator.GenerateAccessToken(new AccessTokenGenerationParameters(
            user.Id,
            user.Email
        ));

        await _verificationSessionManager.RemoveSessionAsync(session.Token); // Remove verification session

        await _unitOfWork.CommitAsync();

        return new AuthResult(refreshToken, accessToken);
    }
}