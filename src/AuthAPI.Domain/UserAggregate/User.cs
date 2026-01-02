using AuthAPI.Domain.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using AuthAPI.Domain.UserAggregate.Entities;

namespace AuthAPI.Domain.UserAggregate;

public class User : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public bool IsVerified { get; private set; } = false;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;


    // Relational properties
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = null!;

    private User() { }

    public static User Create(
        string name,
        string email,
        string password,
        IHasher hasher
    )
    {
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = hasher.Hash(password)
        };

        return user;
    }

    public Result VerifyPassword(string password, IHasher hasher)
    {
        if (PasswordHash is null)
            return Error.Conflict("User does not have a password");

        if (hasher.Verify(password, PasswordHash))
            return Result.Success();
        else
            return Error.Conflict("Wrong password");
    }

    public Result Verify()
    {
        if (IsVerified)
            return Error.Conflict($"User with ID {Id} is already verified");
        IsVerified = true;
        return Result.Success();
    }

    public string AddRefreshToken(ITokenGenerator tokenGenerator, IHasher hasher)
    {
        // Create token
        var refreshTokenStr = tokenGenerator.GenerateRandomToken();
        var refreshToken = RefreshToken.Create(
            hash: hasher.Hash(refreshTokenStr),
            expiresAtUtc: DateTime.UtcNow.AddDays(15),
            userId: Id
        );
        // Add token
        RefreshTokens.Add(refreshToken);
        return refreshTokenStr;
    }
}
