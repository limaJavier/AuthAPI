using AuthAPI.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.Api.Tests.Features.Common.Flows;

public abstract class AbstractFlows(IServiceProvider serviceProvider, HttpClient client)
{
    protected readonly IServiceProvider _serviceProvider = serviceProvider;
    protected readonly HttpClient _client = client;
    protected AuthAPIDbContext ResolveDbContext()
    {
        return _serviceProvider.GetRequiredService<AuthAPIDbContext>();
    }
}
