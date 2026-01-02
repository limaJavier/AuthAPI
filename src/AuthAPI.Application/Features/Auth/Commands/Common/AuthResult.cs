namespace AuthAPI.Application.Features.Auth.Commands.Common;

public record AuthResult(
    string RefreshToken,
    string AccessToken
);