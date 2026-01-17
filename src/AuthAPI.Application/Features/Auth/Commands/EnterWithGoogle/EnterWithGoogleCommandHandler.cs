using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using AuthAPI.Domain.SessionAggregate;
using AuthAPI.Domain.UserAggregate;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.EnterWithGoogle;

public class EnterWithGoogleCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IGoogleValidator googleValidator,
    ITokenGenerator tokenGenerator,
    IHasher hasher
) : ICommandHandler<EnterWithGoogleCommand, Result<AuthResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IGoogleValidator _googleValidator = googleValidator;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result<AuthResult>> Handle(EnterWithGoogleCommand command, CancellationToken cancellationToken)
    {
        // Validate and extract payload from google-token
        var validationResult = await _googleValidator.ValidateAsync(command.GoogleToken);
        if (validationResult.IsFailure)
            return validationResult.Error;
        var googlePayload = validationResult.Value;

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(googlePayload.Email);

        if (user is null) // User does not exist
        {
            // Create a new user
            user = User.Create(
                name: googlePayload.Name,
                email: googlePayload.Email,
                credentialIdentifier: googlePayload.Identifier,
                credentialType: Domain.UserAggregate.Enums.CredentialType.Google
            );
            await _userRepository.AddAsync(user);
        }
        else
        {   
            // Get user's google-credential
            var googleCredential = user.Credentials.FirstOrDefault(credential => credential.Type == Domain.UserAggregate.Enums.CredentialType.Google);

            if (googleCredential is null) // User does not have a google-credential
            {
                // Add credential to user
                var addingResult = user.AddCredential(googlePayload.Identifier, Domain.UserAggregate.Enums.CredentialType.Google);
                if (addingResult.IsFailure)
                    return addingResult.Error;
            }
            else if (googleCredential.Identifier != googlePayload.Identifier) // User's google-credential and google-payload identifiers don't match
            {
                return Error.Unauthorized("User's google-credential's identifier does not match google-payload identifier");
            }
        }

        var (session, refreshToken) = Session.Create(user.Id, _tokenGenerator, _hasher);
        await _sessionRepository.AddAsync(session);
        
        var accessToken = _tokenGenerator.GenerateAccessToken(new AccessTokenGenerationParameters(
            UserId: user.Id,
            SessionId: session.Id
        ));

        await _unitOfWork.CommitAsync();

        return new AuthResult(refreshToken, accessToken);
    }
}
