using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Application.Features.Auth.Events.UserRegistered;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using AuthAPI.Domain.UserAggregate;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.RegisterWithEmail;

public class RegisterWithEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IVerificationSessionManager verificationSessionManager,
    IApplicationEventQueue applicationEventQueue,
    IHasher hasher
) : ICommandHandler<RegisterWithEmailCommand, Result<VerificationResult>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;
    private readonly IApplicationEventQueue _applicationEventQueue = applicationEventQueue;
    private readonly IHasher _hasher = hasher;

    public async ValueTask<Result<VerificationResult>> Handle(RegisterWithEmailCommand command, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(command.Email);

        // Verify user does not exists or is inactive (has been removed)
        if (user is not null && user.IsActive)
            return Error.Conflict($"Email {command.Email} is already registered");

        // Create user
        user = User.Create(
            command.Name,
            command.Email,
            command.Password,
            _hasher
        );
        await _userRepository.AddAsync(user);

        // Create a new verification session with its respective verification-token
        var verificationToken = await _verificationSessionManager.AddSessionAsync(command.Email);

        await _unitOfWork.CommitAsync();

        await _applicationEventQueue.PushAsync(new UserRegisteredEvent(verificationToken));

        return new VerificationResult(verificationToken);
    }
}