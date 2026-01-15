using AuthAPI.Domain.SessionAggregate;
using AuthAPI.Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Infrastructure.Persistence;

public static class AggregateDependenciesIncludes
{
    public static IQueryable<User> IncludeAggregateDependencies(this IQueryable<User> users) =>
        users.Include(user => user.Credentials);

    public static IQueryable<Session> IncludeAggregateDependencies(this IQueryable<Session> sessions) =>
        sessions.Include(session => session.RefreshTokens);
}
