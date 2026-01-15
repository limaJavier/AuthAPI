using AuthAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Infrastructure.Services.Background;

public class ExpiredUnverifiedUserCleanupService(
    IServiceProvider serviceProvider,
    ILogger<ExpiredUnverifiedUserCleanupService> logger
) : BackgroundService
{
    private const int VerificationWindowInHours = 12;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<ExpiredUnverifiedUserCleanupService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Expired Unverified User Cleanup Service started. Running every {Interval}", _interval);

        while (!ct.IsCancellationRequested)
        {
            await CleanupExpiredUnverifiedUsersAsync(ct);

            try
            {
                await Task.Delay(_interval, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Expired Unverified User Cleanup Service stopped");
    }

    private async Task CleanupExpiredUnverifiedUsersAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthAPIDbContext>();

        var expirationDate = DateTime.UtcNow.AddHours(-VerificationWindowInHours);

        try
        {
            var deletedUsersCount = await dbContext.Users
            .Where(u =>
                !u.IsVerified &&
                u.CreatedAtUtc < expirationDate)
            .ExecuteDeleteAsync(ct);

            if (deletedUsersCount == 0)
            {
                _logger.LogDebug("No expired unverified users found to delete");
                return;
            }

            _logger.LogInformation("Successfully deleted {Count} expired unverified users", deletedUsersCount);
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                _logger.LogError(e, "An error occurred while cleaning up expired unverified users");
            }
        }
    }
}
