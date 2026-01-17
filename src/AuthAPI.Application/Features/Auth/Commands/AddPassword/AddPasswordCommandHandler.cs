using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.AddPassword;

public class AddPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IUserContext userContext,
    IHasher hasher
) : ICommandHandler<AddPasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IUserContext _userContext = userContext;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result> Handle(AddPasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId; // Resolve user-id
        var sessionId = _userContext.SessionId; // Resolve session-id

        // Get user by id
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Error.NotFound($"User with ID {userId} does not exist");

        // Add password
        var addPasswordResult = user.AddPassword(command.Password, _hasher);
        if (addPasswordResult.IsFailure)
            return addPasswordResult.Error;

        // Get all other sessions
        var otherSessions = (await _sessionRepository.GetByUserIdAsync(userId))
            .Where(session => session.Id != sessionId);

        // Close all other sessions
        foreach (var session in otherSessions)
        {
            var closingResult = session.Close();
            if (closingResult.IsFailure)
                return closingResult.Error;
        }

        await _unitOfWork.CommitAsync();

        return Result.Success();
    }
}
