using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Infrastructure.Persistence.Repositories;

public class UserRepository(AuthAPIDbContext dbContext) : IUserRepository
{
    private readonly AuthAPIDbContext _dbContext = dbContext;

    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    public Task<User?> GetByEmailAsync(string email) =>
        _dbContext.Users
            .Include(user => user.RefreshTokens)
                .ThenInclude(token => token.ReplacementToken)
            .FirstOrDefaultAsync(user =>
                user.IsActive &&
                user.Email == email
            );

    public Task<User?> GetByIdAsync(Guid id) =>
        _dbContext.Users
            .Include(user => user.RefreshTokens)
                .ThenInclude(token => token.ReplacementToken)
            .FirstOrDefaultAsync(user =>
                user.IsActive &&
                user.Id == id
            );

    public async Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash)
    {
        var token = await _dbContext.RefreshTokens
            .Include(token => token.User)
                .ThenInclude(user => user.RefreshTokens)
                    .ThenInclude(token => token.ReplacementToken)
            .FirstOrDefaultAsync(token => token.Hash == refreshTokenHash);

        return token?.User;
    }
}