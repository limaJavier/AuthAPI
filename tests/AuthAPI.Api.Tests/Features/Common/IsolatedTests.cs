using AuthAPI.Api.Tests.ApiFactories;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Common;

public abstract class IsolatedTests(
    ITestOutputHelper output,
    PostgresContainerFixture postgresContainerFixture
) : IClassFixture<PostgresContainerFixture>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = postgresContainerFixture.Container;
    private IsolatedApiFactory _factory = null!;
    private IServiceScope _scope = null!;
    protected ITestOutputHelper _output = output;
    protected IServiceProvider _serviceProvider = null!;
    protected HttpClient _client = null!;
    protected AuthAPIDbContext _dbContext = null!;

    public virtual async Task InitializeAsync()
    {
        // Resolve dependencies
        var connectionString = await CreateDatabaseAsync(_container);
        _factory = new IsolatedApiFactory(connectionString);
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _serviceProvider = _scope.ServiceProvider;
        _dbContext = _scope.ServiceProvider.GetRequiredService<AuthAPIDbContext>();

        await _dbContext.Database.MigrateAsync(); // Migrate schema
    }

    public virtual async Task DisposeAsync()
    {
        // Dispose dependencies
        await _dbContext.DisposeAsync();
        _scope.Dispose();
        await _factory.DisposeAsync();
    }

    private static async Task<string> CreateDatabaseAsync(PostgreSqlContainer container)
    {
        var dbName = $"test_{Guid.NewGuid():N}";
        var connectionString = container.GetConnectionString();

        await using var dbConnection = new NpgsqlConnection(connectionString);

        await dbConnection.OpenAsync();
        await new NpgsqlCommand(
            $"CREATE DATABASE \"{dbName}\";",
            dbConnection
        ).ExecuteNonQueryAsync();

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = dbName
        };

        return connectionStringBuilder.ToString();
    }
}
