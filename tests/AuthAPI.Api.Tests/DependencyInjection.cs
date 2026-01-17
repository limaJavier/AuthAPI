using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AuthAPI.Api.Tests;

public static class DependencyInjection
{
    public static void ConfigureDataAccess(this IServiceCollection services, string connectionString)
    {
        //** Replace original DbContext registration
        // Get descriptor
        var dbContextDescriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<AuthAPIDbContext>))
            ?? throw new Exception($"Cannot resolve {nameof(AuthAPIDbContext)} injection descriptor");
        services.Remove(dbContextDescriptor); // Remove registration

        // Register new DbContext
        services.AddDbContext<AuthAPIDbContext>((serviceProvider, options) =>
            options.UseNpgsql(connectionString)
        );

        //** Replace original IGoogleValidator registration for a mocked one
        // Get descriptor
        var googleValidatorDescriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(IGoogleValidator))
            ?? throw new Exception($"Cannot resolve {nameof(IGoogleValidator)} injection descriptor");
        services.Remove(googleValidatorDescriptor); // Remove registration

        var googleValidatorMock = new Mock<IGoogleValidator>();
        // Default mock configuration
        googleValidatorMock
            .Setup(validator => validator.ValidateAsync(Constants.User.GoogleToken))
            .ReturnsAsync(new GooglePayload(
                Constants.User.GoogleIdentifier,
                Constants.User.Email,
                Constants.User.Name));
        services.AddSingleton(googleValidatorMock.Object); // Register mocked implementation
    }
}
