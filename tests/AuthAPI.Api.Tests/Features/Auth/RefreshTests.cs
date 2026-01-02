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

        var user = await _dbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        var newRefreshToken = user.RefreshTokens.First(token => hasher.VerifyDeterministic(newRefreshTokenStr, token.Hash));
        var oldRefreshToken = user.RefreshTokens.First(token => hasher.VerifyDeterministic(oldRefreshTokenStr, token.Hash));

        // Assert
        Assert.NotEmpty(response.AccessToken);
        Assert.NotNull(oldRefreshToken!.RevokedAtUtc); // Old refresh-token is revoked
        Assert.Equal(oldRefreshToken.ReplacementTokenId, newRefreshToken.Id); // Old refresh-token was replaced
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task WhenRefreshTokenWasAlreadyReplaced_ShouldRevokeChainAndReturnUnauthorized(int chainLength)
    {
        //** Arrange
        var tokenChain = new List<string>();
        var (_, refreshTokenStr) = await _authFlows.RegisterAndVerifyAsync(); // Register and verify user
        tokenChain.Add(refreshTokenStr);

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
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
            tokenChain.Add(refreshTokenStr);
        }

        // Send a replaced (and revoked) token
        httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Refresh,
            refreshToken: tokenChain[0]
        );

        var user = await _dbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        var revokedChain = user.RefreshTokens.Where(token =>
            tokenChain.Any(tokenStr => hasher.VerifyDeterministic(tokenStr, token.Hash)));

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode); // Unauthorized status code
        Assert.All(revokedChain, token => Assert.NotNull(token.RevokedAtUtc)); // All tokens in the chain are revoked
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