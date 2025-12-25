using System.Net.Http.Json;
using AuthAPI.Api.Features.Items.Responses;
using AuthAPI.Api.Tests.Features.Common;
using AuthAPI.Api.Tests.Fixtures;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Items;

public class GetItemsTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : IsolatedTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task ShouldReturnItems()
    {
        //** Arrange

        //** Act
        var httpResponse = await _client.GetAsync("/items");
        httpResponse.EnsureSuccessStatusCode();
        var response = (await httpResponse.Content.ReadFromJsonAsync<List<ItemResponse>>())!;

        //** Assert
        foreach (var item in response)
            _output.WriteLine(item.Name);
    }
}
