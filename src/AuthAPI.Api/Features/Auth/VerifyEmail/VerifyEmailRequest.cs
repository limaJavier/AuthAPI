namespace AuthAPI.Api.Features.Auth.VerifyEmail;

public record VerifyEmailRequest(
    string VerificationToken,
    string VerificationCode
);