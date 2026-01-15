using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IUnitOfWork unitOfWork,
    ISessionRepository sessionRepository,
    IUserContext userContext
) : ICommandHandler<LogoutCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IUserContext _userContext = userContext;

    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var sessionId = _userContext.SessionId;

        // Get session by ID
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session is null)
            return Error.Unauthorized($"Session with ID {sessionId} does not exist");

        // Close session
        var result = session.Close();
        await _unitOfWork.CommitAsync();

        return result;
    }
}
