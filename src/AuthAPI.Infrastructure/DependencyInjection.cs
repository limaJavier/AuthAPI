using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Infrastructure.Persistence;
using AuthAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add dbContext
        var connectionString = configuration.GetConnectionString(nameof(AuthAPI));
        services.AddDbContext<AuthAPIDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add services
        services.AddScoped<IUnitOfWork>(serviceProvider => 
            serviceProvider.GetRequiredService<AuthAPIDbContext>());

        services.AddScoped<IApplicationEventQueue, ApplicationEventQueue>();

        return services;
    }
}
