using System.Text;
using AuthAPI.Infrastructure.Settings;
using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(configuration);
        services.AddAuth(configuration);
        services.AddFastEndpoints()
            .SwaggerDocument();
        services.AddMappings();

        return services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        // Load the origins from appsettings.json
        var allowedOrigins = configuration
            .GetSection("CorsSettings:AllowedOrigins")
            .Get<string[]>()
            ?? throw new Exception("Cannot resolve the CORS allowed origins");
        if (allowedOrigins.Length == 0)
            throw new Exception("There are no CORS allowed origins");

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static IServiceCollection AddMappings(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(
            typeof(DependencyInjection).Assembly
        );
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }

    private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        // Resolve jtw-settings
        var jwtSettings = configuration.GetRequiredSection(nameof(JwtSettings)).Get<JwtSettings>()
            ?? throw new Exception("Cannot resolve JwtSettings");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)); // Generate symmetric security key

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = securityKey,
            };
        });

        services.AddAuthorization();

        return services;
    }
}
