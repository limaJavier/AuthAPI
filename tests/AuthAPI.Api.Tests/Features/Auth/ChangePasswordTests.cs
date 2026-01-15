using System.Net;
using AuthAPI.Api.Features.Auth.ChangePassword;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class ChangePasswordTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenOldPasswordIsCorrectAndNewPasswordIsValid_ShouldChangePasswordAndCloseAllOtherSessions()
    {
        //** Arrange
        var (accessToken, refreshTokenStr) = await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var request = AuthRequestsFactory.CreateChangePasswordRequest();

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act 
        var httpResponse = await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.ChangePassword,
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
        Assert.True(hasher.Verify(request.NewPassword, user.PasswordHash!)); // Verify new password
        Assert.All(otherSessions, session =>
        {
            Assert.NotNull(session.ClosedAtUtc);
            Assert.Null(session.CurrentRefreshToken);
        }); // All other sessions were closed
    }

    [Fact]
    public async Task WhenOldPasswordIsWrong_ShouldReturnConflict()
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var request = AuthRequestsFactory.CreateChangePasswordRequest(oldPassword: "Wrong" + Constants.User.Password);

        //** Act 
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.ChangePassword,
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
            Routes.Auth.ChangePassword
        );

    [Theory]
    [MemberData(nameof(BadChangePasswordRequests))]
    public async Task WhenRequestIsBad_ShouldReturnBadRequest(ChangePasswordRequest request)
    {
        //** Arrange
        var (accessToken, _) = await _authFlows.RegisterAndVerifyAsync();

        //** Act and assert
        await CommonTestMethods.WhenRequestIsBad_ShouldReturnBadRequest(
            _client,
            HttpMethod.Post,
            Routes.Auth.ChangePassword,
            request,
            accessToken
        );
    }

    public static IEnumerable<object[]> BadChangePasswordRequests()
    {
        foreach (var badPassword in Constants.User.BadPasswords)
        {
            yield return [AuthRequestsFactory.CreateChangePasswordRequest(oldPassword: badPassword)];
            yield return [AuthRequestsFactory.CreateChangePasswordRequest(newPassword: badPassword)];
        }
    }
}
