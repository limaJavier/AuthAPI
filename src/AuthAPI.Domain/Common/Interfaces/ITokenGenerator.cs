namespace AuthAPI.Domain.Common.Interfaces;

public interface ITokenGenerator
{
    string GenerateAccessToken(AccessTokenGenerationParameters parameters);
    string GenerateRandomToken(int size = 64);
}

public record AccessTokenGenerationParameters(
    Guid UserId,
    Guid SessionId
);
