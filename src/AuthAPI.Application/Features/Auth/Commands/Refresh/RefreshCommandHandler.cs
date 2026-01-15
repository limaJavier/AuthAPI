using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.Refresh;

public class RefreshCommandHandler(
    IUnitOfWork unitOfWork,
    ISessionRepository sessionRepository,
    IHasher hasher,
    ITokenGenerator tokenGenerator
) : ICommandHandler<RefreshCommand, Result<AuthResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IHasher _hasher = hasher;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

    public async ValueTask<Result<AuthResult>> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        // Get session by refresh-token
        var refreshTokenHash = _hasher.HashDeterministic(command.RefreshToken);
        var session = await _sessionRepository.GetByRefreshTokenHashAsync(refreshTokenHash);
        if (session is null)
            return Error.Unauthorized($"Refresh-token {command.RefreshToken} does not exist");

        // Generate JWT token
        var accessToken = _tokenGenerator.GenerateAccessToken(new AccessTokenGenerationParameters(
            session.UserId,
            session.Id
        ));

        // Refresh session
        var refreshResult = session.Refresh(command.RefreshToken, _hasher, _tokenGenerator);
        await _unitOfWork.CommitAsync();

        if (refreshResult.IsFailure)
            return refreshResult.Error;

        return new AuthResult(refreshResult.Value, accessToken);
    }
}
