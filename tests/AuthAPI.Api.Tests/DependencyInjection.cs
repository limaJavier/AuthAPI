using AuthAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.Api.Tests;

public static class DependencyInjection
{
    public static void ConfigureDataAccess(this IServiceCollection services, string connectionString)
    {
        // Replace original DbContext registration
        var descriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<AuthAPIDbContext>))
            ?? throw new Exception($"Cannot resolve {nameof(AuthAPIDbContext)} injection descriptor");

        services.Remove(descriptor);

        services.AddDbContext<AuthAPIDbContext>((serviceProvider, options) =>
            options.UseNpgsql(connectionString)
        );
    }
}
