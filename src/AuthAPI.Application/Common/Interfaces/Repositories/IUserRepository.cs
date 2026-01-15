using AuthAPI.Domain.UserAggregate;

namespace AuthAPI.Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    //** Queries
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);

    //** Commands
    Task AddAsync(User user);
}
