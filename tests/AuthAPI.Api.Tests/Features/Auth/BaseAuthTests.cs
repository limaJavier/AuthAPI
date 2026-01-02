using AuthAPI.Api.Tests.Features.Common;
using AuthAPI.Api.Tests.Features.Common.Flows;
using AuthAPI.Api.Tests.Fixtures;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public abstract class BaseAuthTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : IsolatedTests(output, postgresContainerFixture)
{
    protected AuthFlows _authFlows = null!;
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _authFlows = new AuthFlows(_serviceProvider, _client);
    }
}