using System.Net;
using System.Net.Http.Json;
using AuthAPI.Api.Features.Auth.Common.Responses;
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

public class RefreshTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenRefreshTokensIsValid_ShouldRefreshThemAndRevokeAndReplaceOldRefreshToken()
    {
        //** Arrange
        var (_, oldRefreshTokenStr) = await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var httpResponse = await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Refresh,
            refreshToken: oldRefreshTokenStr
        );

        var newRefreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var newRefreshTokenStr = AuthFlows.ExtractTokenFromCookie(newRefreshTokenHeader);
        var response = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        // Get user by email
        var user = await _dbContext.Users
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        // Get refreshed session
        var session = user.Sessions
            .First(session =>
                session.RefreshTokens.Any(token => hasher.VerifyDeterministic(newRefreshTokenStr, token.Hash)));

        // Get old refresh-token
        var oldRefreshToken = session.RefreshTokens.First(token => hasher.VerifyDeterministic(oldRefreshTokenStr, token.Hash));

        // Assert
        Assert.NotEmpty(response.AccessToken);
        Assert.True(hasher.VerifyDeterministic(newRefreshTokenStr, session.CurrentRefreshToken!.Hash));
        Assert.NotNull(oldRefreshToken!.RevokedAtUtc); // Old refresh-token was revoked
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task WhenRefreshTokenWasAlreadyRevoked_ShouldCloseSessionAndReturnUnauthorized(int chainLength)
    {
        //** Arrange
        var (_, firstRefreshToken) = await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var refreshTokenStr = firstRefreshToken;
        HttpResponseMessage httpResponse;
        foreach (var _ in Enumerable.Range(0, chainLength))
        {
            httpResponse = await _client.SendAndEnsureSuccessAsync(
                method: HttpMethod.Post,
                route: Routes.Auth.Refresh,
                refreshToken: refreshTokenStr
            );

            var refreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
            refreshTokenStr = AuthFlows.ExtractTokenFromCookie(refreshTokenHeader);
        }

        // Send a replaced (and revoked) token
        httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Refresh,
            refreshToken: firstRefreshToken
        );

        // Get user by email
        var user = await _dbContext.Users
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        // Get refreshed session
        var session = user.Sessions
            .First(session =>
                session.RefreshTokens.Any(token => hasher.VerifyDeterministic(refreshTokenStr, token.Hash)));

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode); // Unauthorized status code
        Assert.Null(session.CurrentRefreshToken); // Current refresh-token is null
        Assert.NotNull(session.ClosedAtUtc); // Session was closed
    }

    [Fact]
    public async Task WhenRefreshTokensIsMissing_ShouldReturnUnauthorized()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Refresh
        );

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenRefreshTokensDoesNotExist_ShouldReturnUnauthorized()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Refresh,
            refreshToken: "WrongRefreshToken"
        );

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }
}
