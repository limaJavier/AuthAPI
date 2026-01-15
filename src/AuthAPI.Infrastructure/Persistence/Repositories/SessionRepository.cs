using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.SessionAggregate;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Infrastructure.Persistence.Repositories;

public class SessionRepository(AuthAPIDbContext dbContext) : ISessionRepository
{
    private readonly AuthAPIDbContext _dbContext = dbContext;

    public async Task AddAsync(Session session) =>
        await _dbContext.Sessions.AddAsync(session);

    public Task<Session?> GetByIdAsync(Guid sessionId) =>
        _dbContext.Sessions
            .IncludeAggregateDependencies()
            .FirstOrDefaultAsync(session =>
                session.ClosedAtUtc == null &&
                session.Id == sessionId
            );

    public Task<Session?> GetByRefreshTokenHashAsync(string refreshTokenHash) =>
        _dbContext.Sessions
            .IncludeAggregateDependencies()
            .FirstOrDefaultAsync(session =>
                session.ClosedAtUtc == null &&
                session.RefreshTokens.Any(token => token.Hash == refreshTokenHash)
            );

    public Task<List<Session>> GetByUserIdAsync(Guid userId) =>
        _dbContext.Sessions
            .IncludeAggregateDependencies()
            .Where(session =>
                session.ClosedAtUtc == null &&
                session.UserId == userId
            )
            .ToListAsync();
}
