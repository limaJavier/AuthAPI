using AuthAPI.Domain.Common;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using AuthAPI.Domain.SessionAggregate.Entities;
using AuthAPI.Domain.UserAggregate;

namespace AuthAPI.Domain.SessionAggregate;

public class Session : Entity
{
    // IP Addresses
    // Countries
    // Type (email/password, google, apple, github, etc.)
    public DateTime? ClosedAtUtc { get; private set; }
    public RefreshToken? CurrentRefreshToken => RefreshTokens.FirstOrDefault(token => token.RevokedAtUtc is null);

    // Relational properties
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = null!; // Belongs to session aggregate
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;


    private Session() { }

    public static (Session session, string refreshTokenStr) Create(Guid userId, ITokenGenerator tokenGenerator, IHasher hasher)
    {
        // Create token
        var session = new Session
        {
            UserId = userId,
            RefreshTokens = []
        };

        // Create refresh-token
        var refreshTokenStr = tokenGenerator.GenerateRandomToken();
        var refreshToken = RefreshToken.Create(
            hash: hasher.HashDeterministic(refreshTokenStr),
            expiresAtUtc: DateTime.UtcNow.AddDays(15),
            sessionId: session.Id
        );
        session.RefreshTokens.Add(refreshToken);

        return (session, refreshTokenStr);
    }

    public Result Close()
    {
        if (ClosedAtUtc is not null)
            return Error.Unauthorized($"Session with ID {Id} was already closed");

        var revocationResult = CurrentRefreshToken!.Revoke();
        if (revocationResult.IsFailure)
            return revocationResult;

        ClosedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }

    public Result<string> Refresh(string refreshTokenStr, IHasher hasher, ITokenGenerator tokenGenerator)
    {
        if (ClosedAtUtc is not null)
            return Error.Unauthorized($"Session with ID {Id} was already closed");

        // Get refresh-token by hash
        var refreshToken = RefreshTokens.FirstOrDefault(token => hasher.VerifyDeterministic(refreshTokenStr, token.Hash));
        if (refreshToken is null) // Token does not exist
        {
            return Error.Unauthorized($"Refresh-token {refreshTokenStr} does not exist");
        }
        else if (refreshToken.RevokedAtUtc is not null) // A revoked token is being used (this can be a replay attack)
        {
            // Close the session
            var closingResult = Close();
            return closingResult.IsFailure ?
                closingResult.Error :
                Error.Unauthorized($"Refresh-token {refreshTokenStr} with ID {refreshToken.Id} was already revoked and replaced ");
        }

        // Create new refresh-token
        var newTokenStr = tokenGenerator.GenerateRandomToken();
        var newToken = RefreshToken.Create(
            hash: hasher.HashDeterministic(newTokenStr),
            expiresAtUtc: DateTime.UtcNow.AddDays(15),
            sessionId: Id
        );

        // Revoke old refresh-token
        var revocationResult = CurrentRefreshToken!.Revoke();
        if (revocationResult.IsFailure)
            return revocationResult.Error;

        // Add new refresh-token
        RefreshTokens.Add(newToken);

        return newTokenStr;
    }
}
