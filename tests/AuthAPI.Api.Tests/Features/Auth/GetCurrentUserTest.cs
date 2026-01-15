using AuthAPI.Api.Features.Auth.GetCurrentUser;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class GetCurrentUserTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenUserIsLoggedIn_ShouldReturnUserResponse()
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.RegisterAndVerifyAsync();

        //** Act
        var response = await _client.SendAsync<UserResponse>(
            method: HttpMethod.Get,
            route: Routes.Auth.Me,
            accessToken: accessToken
        );

        //** Assert
        Assert.Equal(Constants.User.Name, response.Name);
        Assert.Equal(Constants.User.Email, response.Email);
    }

    [Fact]
    public Task WhenAccessTokenIsMissing_ShouldReturnUnauthorized() =>
        CommonTestMethods.WhenAccessTokenIsMissing_ShouldReturnUnauthorized(
            _client,
            HttpMethod.Get,
            Routes.Auth.Me
        );
}
