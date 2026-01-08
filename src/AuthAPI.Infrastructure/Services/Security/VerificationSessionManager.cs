using AuthAPI.Application.Common;
using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Domain.Common.Interfaces;

namespace AuthAPI.Infrastructure.Services.Security;

// TODO: This implementation should use Redis
public class VerificationSessionManager(ITokenGenerator tokenGenerator) : IVerificationSessionManager
{
    private static readonly HashSet<VerificationSession> _sessions = [];
    private static readonly Lock _lock = new();
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

    public async Task<VerificationSession?> GetSessionAsync(string token)
    {
        await Task.CompletedTask;

        VerificationSession? session = null;
        lock (_lock)
        {
            _sessions.RemoveWhere(session => session.ExpiresAtUtc < DateTime.UtcNow);
            session = _sessions.FirstOrDefault(session => session.Token == token);
        }

        return session;
    }

    public async Task<string> AddSessionAsync(string email)
    {
        await Task.CompletedTask;

        var token = _tokenGenerator.GenerateRandomToken(32);
        var code = Random.Shared.Next(100000, 999999).ToString(); // Generate a 6 digits string

        lock (_lock)
        {
            // TODO: Expiry datetime should not be hardcoded
            _sessions.Add(new(
                token,
                email,
                code,
                DateTime.UtcNow.AddMinutes(10)
            ));
        }

        return token;
    }

    public async Task UpdateSessionAsync(VerificationSession session)
    {
        await Task.CompletedTask;

        lock (_lock)
        {
            var storedSession = _sessions
            .FirstOrDefault(storedSession => storedSession.Token == session.Token)
            ?? throw new Exception($"Session with token {session.Token} was not found");

            _sessions.Remove(storedSession);
            _sessions.Add(session);
        }
    }

    public async Task RemoveSessionAsync(string token)
    {
        await Task.CompletedTask;

        lock (_lock)
        {
            var count = _sessions.RemoveWhere(session => session.Token == token);
            if (count == 0)
                throw new Exception($"Cannot remove session: Session with token {token} was not found");
        }
    }
}
