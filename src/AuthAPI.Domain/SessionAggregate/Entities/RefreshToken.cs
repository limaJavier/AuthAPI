using AuthAPI.Domain.Common;
using AuthAPI.Domain.Common.Results;

namespace AuthAPI.Domain.SessionAggregate.Entities;

public class RefreshToken : Entity
{
    public string Hash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    // Relational properties
    public Guid SessionId { get; private set; }
    public Session Session { get; private set; } = null!;

    private RefreshToken() { }

    internal static RefreshToken Create(
        string hash,
        DateTime expiresAtUtc,
        Guid sessionId
    )
    {
        var token = new RefreshToken
        {
            Hash = hash,
            ExpiresAtUtc = expiresAtUtc,
            SessionId = sessionId
        };

        return token;
    }

    internal Result Revoke()
    {
        // Verify it has not been already revoked
        if (RevokedAtUtc is not null)
            return Error.Conflict($"Refresh-token with ID {Id} was already revoked");

        RevokedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
}
