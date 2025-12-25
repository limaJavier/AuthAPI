using Testcontainers.PostgreSql;

namespace AuthAPI.Api.Tests.Fixtures;

public class PostgresContainerFixture : IAsyncLifetime
{
    public readonly PostgreSqlContainer Container = new PostgreSqlBuilder()
        .WithImage("postgres:18.1")
        .Build();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
