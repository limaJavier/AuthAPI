using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddAuth();
        
        services.AddFastEndpoints()
            .SwaggerDocument();

        services.AddMappings();
        
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

    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
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

                ValidIssuer = "api.auth",
                ValidAudience = "api",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("An at least 256 bits signing key")), // TODO: Save credential info in a secure place
            };
        });

        services.AddAuthorization();

        return services;
    }
}
