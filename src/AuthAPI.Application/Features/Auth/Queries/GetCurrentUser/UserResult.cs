namespace AuthAPI.Application.Features.Auth.Queries.GetCurrentUser;

public record UserResult(
    string Name,
    string Email
);
