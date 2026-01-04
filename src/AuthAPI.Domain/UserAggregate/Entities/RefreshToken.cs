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
        // Validate token state
        var validationResult = Validate();
        if (validationResult.IsFailure)
            return validationResult.Error;

        // Replace
        ReplacementTokenId = replacementTokenId;
        RevokedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    internal Result Revoke()
    {
        // Validate token state
        var validationResult = Validate();
        if (validationResult.IsFailure)
            return validationResult.Error;

        RevokedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    internal void RevokeWithoutValidation()
    {
        RevokedAtUtc ??= DateTime.UtcNow;
    }

    private Result Validate()
    {
        // Verify token has a replacement
        if (ReplacementTokenId is not null)
        {
            // If the token already has a replacement then this can be a signal of a refresh token replay attack
            // Therefore the whole token chain must be revoked immediately
            RevokeChain();
            return Error.Unauthorized("Cannot use a revoked and replaced refresh-token");
        }

        // Verify token is expired, if so then the user is unauthorized
        if (ExpiresAtUtc <= DateTime.UtcNow)
            return Error.Unauthorized("Refresh-token is expired");

        // Verify token is revoked, if so then user is unauthorized
        if (RevokedAtUtc is not null)
            return Error.Unauthorized("Refresh-token is revoked");

        return Result.Success();
    }

    private void RevokeChain()
    {
        var token = this;
        token.RevokedAtUtc ??= DateTime.UtcNow;
        while (token.ReplacementToken is not null)
        {
            token.ReplacementToken.RevokedAtUtc ??= DateTime.UtcNow;
            token = token.ReplacementToken;
        }
    }
}