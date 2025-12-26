using AuthAPI.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.Assemblies = [
                typeof(Domain.DependencyInjection).Assembly,
                typeof(DependencyInjection).Assembly
            ];
            options.ServiceLifetime = ServiceLifetime.Scoped;

            options.PipelineBehaviors = [
                typeof(ValidationBehavior<,>),
                // typeof(ValidationBehavior),
            ];
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly); // Scan validators

        return services;
    }
}
