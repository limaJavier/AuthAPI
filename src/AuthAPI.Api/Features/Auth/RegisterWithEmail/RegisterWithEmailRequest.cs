namespace AuthAPI.Api.Features.Auth.RegisterWithEmail;

public record RegisterWithEmailRequest(
    string Name,
    string Email,
    string Password
);