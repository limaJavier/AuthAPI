using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using AuthAPI.Domain.SessionAggregate;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IVerificationSessionManager verificationSessionManager,
    ITokenGenerator tokenGenerator,
    IHasher hasher
) : ICommandHandler<VerifyEmailCommand, Result<AuthResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result<AuthResult>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        //** Verify session
        var verificationSession = await _verificationSessionManager.GetSessionAsync(command.VerificationToken);
        if (verificationSession is null)
            return Error.Unauthorized($"Verification-Session with token {command.VerificationToken} does not exist");

        var verificationResult = verificationSession.Verify(command.VerificationCode);
        if (verificationResult.IsFailure)
            return verificationResult.Error;

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(verificationSession.Email);
        if (user is null)
            return Error.Conflict($"User with email {verificationSession.Email} was not found");

        user.Verify(); // Verify user

        // Create session
        var (session, refreshToken) = Session.Create(user.Id, _tokenGenerator, _hasher);
        await _sessionRepository.AddAsync(session);

        // Generate JWT token
        var accessToken = _tokenGenerator.GenerateAccessToken(new AccessTokenGenerationParameters(
            user.Id,
            session.Id
        ));

        await _verificationSessionManager.RemoveSessionAsync(verificationSession.Token); // Remove verification session

        await _unitOfWork.CommitAsync();

        return new AuthResult(refreshToken, accessToken);
    }
}
