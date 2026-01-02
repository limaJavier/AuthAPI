using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.Refresh;

public class RefreshCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IHasher hasher,
    ITokenGenerator tokenGenerator
) : ICommandHandler<RefreshCommand, Result<AuthResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHasher _hasher = hasher;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

    public async ValueTask<Result<AuthResult>> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        //** Get user by refresh-token
        var refreshTokenHash = _hasher.HashDeterministic(command.RefreshToken);
        var userResult = await _userRepository.GetByRefreshTokenHashAsync(refreshTokenHash);
        if (userResult.IsFailure)
            return userResult.Error;
        var user = userResult.Value;

        // Replace refresh token
        var result = user.ReplaceRefreshToken(command.RefreshToken, _hasher, _tokenGenerator);
        if (result.IsFailure)
            return result.Error;

        // Generate JWT token
        var accessToken = _tokenGenerator.GenerateAccessToken(new AccessTokenGenerationParameters(
            user.Id,
            user.Email
        ));

        await _unitOfWork.CommitAsync();

        return new AuthResult(result.Value, accessToken);
    }
}