using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Results;
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
            .FirstOrDefaultAsync(user =>
                user.IsActive &&
                user.Email == email
            );

    public Task<User?> GetByIdAsync(Guid id) =>
        _dbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user =>
                user.IsActive &&
                user.Id == id
            );

    public async Task<Result<User>> GetByRefreshTokenHashAsync(string refreshTokenHash)
    {
        var token = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.Hash == refreshTokenHash);

        if (token is null)
            return Error.NotFound($"Refresh-Token with hash {refreshTokenHash} was not found");
        else if (token.RevokedAtUtc is not null)
            return Error.Conflict($"Refresh-Token with hash {refreshTokenHash} is revoked");
        else if (token.ExpiresAtUtc < DateTime.UtcNow)
            return Error.Conflict($"Refresh-Token with hash {refreshTokenHash} is expired");
        else if (!token.User.IsActive)
            return Error.Conflict($"Refresh-Token with hash {refreshTokenHash} belongs to an inactive user");

        return token.User;
    }
}