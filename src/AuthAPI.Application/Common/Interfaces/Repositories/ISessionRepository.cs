using AuthAPI.Domain.SessionAggregate;

namespace AuthAPI.Application.Common.Interfaces.Repositories;

public interface ISessionRepository
{
    //** Queries
    Task<Session?> GetByIdAsync(Guid sessionId);
    Task<Session?> GetByRefreshTokenHashAsync(string refreshTokenHash);
    Task<List<Session>> GetByUserIdAsync(Guid userId);

    //** Commands
    Task AddAsync(Session session);
}
