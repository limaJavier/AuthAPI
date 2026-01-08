namespace AuthAPI.Api.Features.Auth.GetCurrentUser;

public record UserResponse(
    string Name,
    string Email
);
