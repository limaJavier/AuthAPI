using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IUserContext userContext,
    IHasher hasher
) : ICommandHandler<ChangePasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserContext _userContext = userContext;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId; // Resolver user-id

        // Get user by id
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Error.NotFound($"User with ID {userId} does not exist");

        // Verify old password
        var verificationResult = user.VerifyPassword(command.OldPassword, _hasher);
        if (verificationResult.IsFailure)
            return verificationResult.Error;

        // Change to new password
        var changePasswordResult = user.ChangePassword(command.NewPassword, _hasher);
        if (changePasswordResult.IsFailure)
            return changePasswordResult.Error;

        user.RevokeAllRefreshTokens();
        await _unitOfWork.CommitAsync();

        return Result.Success();
    }
}