using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.LoginWithEmail;

public class LoginWithEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ITokenGenerator tokenGenerator,
    IHasher hasher
) : ICommandHandler<LoginWithEmailCommand, Result<AuthResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userReponsitory = userRepository;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result<AuthResult>> Handle(LoginWithEmailCommand command, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _userReponsitory.GetByEmailAsync(command.Email);
        if (user is null)
            return Error.NotFound($"User with email {command.Email} does not exist");

        // Check user is verified
        if (!user.IsVerified)
            return Error.Conflict($"User with email {command.Email} is not verified");

        // Verify password
        var result = user.VerifyPassword(command.Password, _hasher);
        if(result.IsFailure)
            return result.Error;

        var refreshToken = user.AddRefreshToken(_tokenGenerator, _hasher); // Generate refresh-token

        // Generate JWT token
        var accessToken = _tokenGenerator.GenerateAccessToken(new AccessTokenGenerationParameters(
            user.Id,
            user.Email
        ));

        await _unitOfWork.CommitAsync();

        return new AuthResult(refreshToken, accessToken);
    }
}