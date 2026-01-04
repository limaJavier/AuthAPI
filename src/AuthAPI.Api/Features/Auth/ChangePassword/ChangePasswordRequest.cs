namespace AuthAPI.Api.Features.Auth.ChangePassword;

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword
);