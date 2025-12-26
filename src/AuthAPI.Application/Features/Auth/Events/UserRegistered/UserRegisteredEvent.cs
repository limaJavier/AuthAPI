using AuthAPI.Application.Common.Interfaces;

namespace AuthAPI.Application.Features.Auth.Events.UserRegistered;

public record UserRegisteredEvent(
    string VerificationToken
) : IApplicationEvent;