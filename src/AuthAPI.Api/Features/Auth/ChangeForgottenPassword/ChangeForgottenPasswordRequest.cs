namespace AuthAPI.Api.Features.Auth.ChangeForgottenPassword;

public record ChangeForgottenPasswordRequest(
    string VerificationToken,
    string Password
);