using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Infrastructure.Persistence;
using AuthAPI.Infrastructure.Persistence.Repositories;
using AuthAPI.Infrastructure.Services;
using AuthAPI.Infrastructure.Services.Background;
using AuthAPI.Infrastructure.Services.Security;
using AuthAPI.Infrastructure.Settings;
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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IApplicationEventQueue, ApplicationEventQueue>();
        services.AddSingleton<IHasher, Hasher>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<IVerificationSessionManager, VerificationSessionManager>();
        services.AddSingleton<IEmailSender, EmailSender>();

        services.AddHostedService<ExpiredUnverifiedUserCleanupService>();

        services.AddSettings(configuration);

        return services;
    }

    private static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection(nameof(JwtSettings))
        );

        // services.Configure<EmailSettings>(
        //     configuration.GetSection(nameof(EmailSettings))
        // );

        return services;
    }
}
