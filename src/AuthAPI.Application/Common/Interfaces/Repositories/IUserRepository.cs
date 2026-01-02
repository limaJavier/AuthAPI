using AuthAPI.Domain.Common.Results;
using AuthAPI.Domain.UserAggregate;

namespace AuthAPI.Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    //** Queries
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<Result<User>> GetByRefreshTokenHashAsync(string refreshTokenHash);

    //** Commands
    Task AddAsync(User user);
}