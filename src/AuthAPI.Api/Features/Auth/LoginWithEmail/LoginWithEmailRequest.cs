namespace AuthAPI.Api.Features.Auth.LoginWithEmail;

public record LoginWithEmailRequest(
    string Email,
    string Password
);