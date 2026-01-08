using AuthAPI.Api.Exceptions;
using FastEndpoints;
using FastEndpoints.Swagger;

namespace AuthAPI.Api;

public static class Pipeline
{
    public static IApplicationBuilder AddPresentation(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(app => app.Run(GlobalExceptionHandler.Handle));
        app.UseCors();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints()
            .UseSwaggerGen();
        return app;
    }
}
