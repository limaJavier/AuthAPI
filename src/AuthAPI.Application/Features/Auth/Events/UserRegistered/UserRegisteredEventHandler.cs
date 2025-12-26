using AuthAPI.Application.Common.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.Features.Auth.Events.UserRegistered;

public class UserRegisteredEventHandler(
    IVerificationSessionManager verificationSessionManager,
    IEmailSender emailSender,
    ILogger<UserRegisteredEventHandler> logger
) : INotificationHandler<UserRegisteredEvent>
{
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<UserRegisteredEventHandler> _logger = logger;

    public async ValueTask Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var session = await _verificationSessionManager.GetSessionAsync(notification.VerificationToken);

        if (session is null)
        {
            _logger.LogError("Session with verification token {VerificationToken} was not found", notification.VerificationToken);
            return;
        }

        var result = await _emailSender.SendAsync(session.Email, session.Code);

        if (result.IsFailure)
        {
            _logger.LogError("Cannot send verification-email to {}: {ErrorMessage}", session.Email, result.Error);
        }
        else
        {
            _logger.LogInformation("A verification-email was sent to {Email} with code {Code}", session.Email, session.Code);
        }
    }
}