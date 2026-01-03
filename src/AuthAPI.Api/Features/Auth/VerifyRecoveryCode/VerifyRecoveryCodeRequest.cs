namespace AuthAPI.Api.Features.Auth.VerifyRecoveryCode;

public record VerifyRecoveryCodeRequest(
    string VerificationToken,
    string VerificationCode
);