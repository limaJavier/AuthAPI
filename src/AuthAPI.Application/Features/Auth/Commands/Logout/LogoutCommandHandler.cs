using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IHasher hasher
) : ICommandHandler<LogoutCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        // Get user by refresh-token
        var refreshTokenHash = _hasher.HashDeterministic(command.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(refreshTokenHash);
        if (user is null)
            return Error.Unauthorized($"Refresh-Token {command.RefreshToken} does not exist");

        // Revoke refresh-token
        var result = user.RevokeRefreshToken(command.RefreshToken, _hasher);
        await _unitOfWork.CommitAsync();

        return result;
    }
}