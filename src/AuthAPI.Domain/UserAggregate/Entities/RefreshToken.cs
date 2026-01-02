using AuthAPI.Domain.Common;
using AuthAPI.Domain.Common.Results;

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

    internal Result Replace(Guid replacementTokenId)
    {
        if (ReplacementTokenId is not null)
            return Error.Conflict("Refresh-Token has been already replaced");
        else if (RevokedAtUtc is not null)
            return Error.Conflict("Refresh-Token has been revoked");
        else if(ExpiresAtUtc < DateTime.UtcNow)
            return Error.Conflict("Refresh-Token is expired");

        ReplacementTokenId = replacementTokenId;
        RevokedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
}