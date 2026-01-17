using System.Net;
using AuthAPI.Api.Features.Auth.AddPassword;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class AddPasswordTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task WhenUserWasRegisteredAndDoesNotHaveAPassword_ShouldAddPasswordAndCloseAllOtherSessions(int openSessions)
    {
        //** Arrange
        var (accessToken, refreshTokenStr) = await _authFlows.EnterWithGoogleAsync();

        // Login several times (i.e create several sessions)
        foreach (var _ in Enumerable.Range(0, openSessions))
            await _authFlows.EnterWithGoogleAsync();

        var request = AuthRequestsFactory.CreateAddPasswordRequest();

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.AddPassword,
            body: request,
            accessToken: accessToken
        );

        var user = await _dbContext.Users
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        // Get other sessions
        var otherSessions = user.Sessions
            .Where(session =>
                !session.RefreshTokens.Any(token => hasher.VerifyDeterministic(refreshTokenStr, token.Hash)));

        //** Assert
        Assert.True(hasher.Verify(request.Password, user.PasswordHash!)); // Verify new password
        Assert.Single(user.Sessions, session =>
            session.ClosedAtUtc is null &&
            hasher.VerifyDeterministic(refreshTokenStr, session.CurrentRefreshToken!.Hash)
        ); // Only one session is not closed
        Assert.All(otherSessions, session =>
        {
            Assert.NotNull(session.ClosedAtUtc);
            Assert.Null(session.CurrentRefreshToken);
        }); // All other sessions were closed
    }

    [Fact]
    public async Task WhenPasswordWasAlreadyAdded_ShouldReturnConflict()
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.EnterWithGoogleAsync(); // Enter with google

        var request = AuthRequestsFactory.CreateAddPasswordRequest();

        // Add password
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.AddPassword,
            body: request,
            accessToken: accessToken
        );

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.AddPassword,
            body: request,
            accessToken: accessToken
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenUserRegisteredWithEmail_ShouldReturnConflict()
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var request = AuthRequestsFactory.CreateAddPasswordRequest();

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.AddPassword,
            body: request,
            accessToken: accessToken
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact]
    public Task WhenAccessTokenIsMissing_ShouldReturnUnauthorized() =>
        CommonTestMethods.WhenAccessTokenIsMissing_ShouldReturnUnauthorized(
            _client,
            HttpMethod.Post,
            Routes.Auth.AddPassword
        );

    [Theory]
    [MemberData(nameof(BadAddPasswordRequests))]
    public async Task WhenRequestIsBad_ShouldReturnBadRequest(AddPasswordRequest request)
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.EnterWithGoogleAsync();

        //** Act and assert
        await CommonTestMethods.WhenRequestIsBad_ShouldReturnBadRequest(
            _client,
            HttpMethod.Post,
            Routes.Auth.AddPassword,
            request,
            accessToken
        );
    }

    public static IEnumerable<object[]> BadAddPasswordRequests()
    {
        foreach (var badPassword in Constants.User.BadPasswords)
            yield return [AuthRequestsFactory.CreateAddPasswordRequest(password: badPassword)];
    }
}
