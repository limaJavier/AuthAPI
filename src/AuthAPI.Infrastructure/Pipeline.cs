using AuthAPI.Infrastructure.Middlewares;
using AuthAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.Infrastructure;

public static class Pipeline
{
    public static IApplicationBuilder AddInfrastructure(this IApplicationBuilder app)
    {
        app.UseMiddleware<EventualConsistencyMiddleware>();
        app.ApplyMigrations();
        return app;
    }

    private static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AuthAPIDbContext>();
        dbContext.Database.Migrate();
    }
}
