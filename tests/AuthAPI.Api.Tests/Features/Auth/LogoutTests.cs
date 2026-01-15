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
    public async Task WhenSessionIsOpen_ShouldCloseIt()
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

        // Get user by email
        var user = await _dbContext.Users
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        // Get logout session
        var session = user.Sessions
            .First(session =>
                session.RefreshTokens.Any(token => hasher.VerifyDeterministic(refreshTokenStr, token.Hash)));

        //** Assert
        Assert.Null(session.CurrentRefreshToken); // There's no current refresh token, because session was closed
        Assert.NotNull(session.ClosedAtUtc); // Session was closed
        Assert.Empty(newRefreshTokenStr); // New refresh token should be empty (it must be removed)
    }

    [Fact]
    public async Task WhenLoggingOutTwice_ShouldReturnUnauthorized()
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.RegisterAndVerifyAsync();

        //** Act

        // Logout
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Logout,
            accessToken: accessToken
        );

        // Logout again
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Logout,
            accessToken: accessToken
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
