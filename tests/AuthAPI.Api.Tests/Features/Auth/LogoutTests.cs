using System.Net;
using AuthAPI.Api.Tests.Features.Common.Flows;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class LogoutTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenRefreshTokenIsValid_ShouldRevokeIt()
    {
        //** Arrange
        var (accessToken, refreshTokenStr) = await _authFlows.RegisterAndVerifyAsync();

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var httpResponse = await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Logout,
            accessToken: accessToken,
            refreshToken: refreshTokenStr
        );

        var newRefreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var newRefreshTokenStr = AuthFlows.ExtractTokenFromCookie(newRefreshTokenHeader);

        var user = await _dbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        var refreshToken = user.RefreshTokens.First(token => hasher.VerifyDeterministic(refreshTokenStr, token.Hash));

        //** Assert
        Assert.NotNull(refreshToken.RevokedAtUtc); // Token was revoked
        Assert.Empty(newRefreshTokenStr); // New refresh token should be empty (it must be removed)
    }

    [Fact]
    public async Task WhenLoggingOutTwiceWithTheSameRefreshToken_ShouldReturnUnauthorized()
    {
        //** Arrange
        var (accessToken, refreshTokenStr) = await _authFlows.RegisterAndVerifyAsync();

        //** Act

        // Logout
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Logout,
            accessToken: accessToken,
            refreshToken: refreshTokenStr
        );

        // Logout again
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Logout,
            accessToken: accessToken,
            refreshToken: refreshTokenStr
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenRefreshTokenIsMissing_ShouldReturnUnauthorized()
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.RegisterAndVerifyAsync();

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Logout,
            accessToken: accessToken
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenRefreshTokenIsWrong_ShouldReturnUnauthorized()
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.RegisterAndVerifyAsync();

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Logout,
            accessToken: accessToken,
            refreshToken: "WrongRefreshToken"
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public Task WhenAccessTokenIsMissing_ShouldReturnUnauthorized() =>
        CommonTestMethods.WhenAccessTokenIsMissing_ShouldReturnUnauthorized(
            _client,
            HttpMethod.Post,
            Routes.Auth.Logout
        );
}
