using AuthAPI.Application.Common.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Application.Features.Auth.Events.Verification;

public class VerificationEventHandler(
    IVerificationSessionManager verificationSessionManager,
    IEmailSender emailSender,
    ILogger<VerificationEventHandler> logger
) : INotificationHandler<VerificationEvent>
{
    private readonly IVerificationSessionManager _verificationSessionManager = verificationSessionManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<VerificationEventHandler> _logger = logger;

    public async ValueTask Handle(VerificationEvent verificationEvent, CancellationToken cancellationToken)
    {
        var session = await _verificationSessionManager.GetSessionAsync(verificationEvent.VerificationToken);

        if (session is null)
        {
            _logger.LogError("Session with verification token {VerificationToken} was not found", verificationEvent.VerificationToken);
            return;
        }

        var result = await _emailSender.SendAsync(
            toEmail: session.Email,
            subject: verificationEvent.Type == VerificationEventType.Email ? "Email Verification Code" : "Password Recovery Code",
            body: session.Code);

        if (result.IsFailure)
        {
            _logger.LogError("Cannot send verification-email of type {Type} to {Email}: {Error}", verificationEvent.Type, session.Email, result.Error);
        }
        else
        {
            _logger.LogInformation("A verification-email of type {Type} was sent to {Email} with code {Code}", verificationEvent.Type, session.Email, session.Code);
        }
    }
}
