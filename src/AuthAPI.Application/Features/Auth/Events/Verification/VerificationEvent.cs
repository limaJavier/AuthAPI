using AuthAPI.Application.Common.Interfaces;

namespace AuthAPI.Application.Features.Auth.Events.Verification;

public record VerificationEvent(
    string VerificationToken,
    VerificationEventType Type
) : IApplicationEvent;

public enum VerificationEventType
{
    Email,
    Password
}
