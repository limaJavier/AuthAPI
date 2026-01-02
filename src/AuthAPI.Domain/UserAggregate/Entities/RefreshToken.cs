using AuthAPI.Domain.Common;

namespace AuthAPI.Domain.UserAggregate.Entities;

public class RefreshToken : Entity
{
    public string Hash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    // Relational properties
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid? ReplacementTokenId { get; private set; }
    public RefreshToken? ReplacementToken { get; private set; } = null!;

    private RefreshToken() { }

    internal static RefreshToken Create(
        string hash,
        DateTime expiresAtUtc,
        Guid userId
    )
    {
        var token = new RefreshToken
        {
            Hash = hash,
            ExpiresAtUtc = expiresAtUtc,
            UserId = userId
        };

        return token;
    }
}