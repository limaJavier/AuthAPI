using AuthAPI.Api.Tests.ApiFactories;
using AuthAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Common;

public class SharedTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    protected readonly ITestOutputHelper _output;
    protected readonly HttpClient _client;
    protected readonly IServiceScope _scope;
    protected readonly AuthAPIDbContext _dbContext;

    public SharedTests(ITestOutputHelper output, ApiFactory factory)
    {
        _output = output;
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AuthAPIDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        _scope.Dispose();
    }
}
