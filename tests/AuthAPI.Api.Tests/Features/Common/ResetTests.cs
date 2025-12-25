using AuthAPI.Api.Tests.ApiFactories;
using AuthAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Common;

public class ResetTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    protected readonly ITestOutputHelper _output;
    protected readonly HttpClient _client;
    protected readonly IServiceScope _scope;
    protected readonly AuthAPIDbContext _dbContext;

    public ResetTests(ITestOutputHelper output, ApiFactory factory)
    {
        _output = output;
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AuthAPIDbContext>();
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        _scope.Dispose();
    }

    private async Task ResetDatabaseAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
        @"
            DO $$
            DECLARE
                s RECORD;
            BEGIN
                FOR s IN
                    SELECT nspname
                    FROM pg_namespace
                    WHERE nspname NOT IN (
                        'pg_catalog',
                        'information_schema',
                        'pg_toast'
                    )
                    AND nspname NOT LIKE 'pg_temp_%'
                    AND nspname NOT LIKE 'pg_toast_temp_%'
                LOOP
                    EXECUTE format('DROP SCHEMA %I CASCADE', s.nspname);
                    EXECUTE format('CREATE SCHEMA %I', s.nspname);
                END LOOP;
            END $$;
        ");
    }
}
