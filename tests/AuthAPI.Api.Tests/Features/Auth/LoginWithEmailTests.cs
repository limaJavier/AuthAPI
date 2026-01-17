using System.Net;
using System.Net.Http.Json;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.LoginWithEmail;
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

public class LoginWithEmailTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenUserWithEmailExistsIsVerifiedAndPasswordMatch_ShouldCreateASessionAndReturnAccessAndRefreshTokens()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var request = AuthRequestsFactory.CreateLoginRequest(); // Create request

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var httpResponse = await _client.SendAndEnsureSuccessAsync(HttpMethod.Post, Routes.Auth.Login, request);

        var refreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var refreshTokenStr = AuthFlows.ExtractTokenFromCookie(refreshTokenHeader);
        var response = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        var user = await _dbContext.Users
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        //** Assert
        Assert.NotNull(refreshTokenHeader);
        Assert.NotEmpty(refreshTokenHeader);
        Assert.NotEmpty(response.AccessToken);
        Assert.Contains(user.Sessions, session => hasher.VerifyDeterministic(refreshTokenStr, session.CurrentRefreshToken!.Hash));
    }

    [Fact]
    public async Task WhenUserWithEmailDoesNotExist_ShouldReturnNotFound()
    {
        //** Arrange
        var request = AuthRequestsFactory.CreateLoginRequest();

        //** Act
        var httpResponse = await _client.SendAsync(HttpMethod.Post, Routes.Auth.Login, request);

        //** Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenUserWithEmailExistsButIsNotVerified_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAsync(); // Register user

        var loginRequest = AuthRequestsFactory.CreateLoginRequest();

        //** Act
        var httpResponse = await _client.SendAsync(HttpMethod.Post, Routes.Auth.Login, loginRequest);

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenUserWithEmailExistsIsVerifiedButPasswordDoesNotMatch_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var loginRequest = AuthRequestsFactory.CreateLoginRequest(password: Constants.User.Password + "Wrong");

        //** Act
        var loginHttpResponse = await _client.PostAsJsonAsync(Routes.Auth.Login, loginRequest);

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, loginHttpResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(BadLoginWithEmailRequests))]
    public Task WhenRequestIsBad_ShouldReturnBadRequest(LoginWithEmailRequest request)
        => CommonTestMethods.WhenRequestIsBad_ShouldReturnBadRequest(
            _client,
            HttpMethod.Post,
            Routes.Auth.Login,
            request
        );

    public static IEnumerable<object[]> BadLoginWithEmailRequests()
    {
        foreach (var badEmail in Constants.User.BadEmails)
            yield return [AuthRequestsFactory.CreateLoginRequest(email: badEmail)];

        foreach (var badPassword in Constants.User.BadPasswords)
            yield return [AuthRequestsFactory.CreateLoginRequest(password: badPassword)];
    }
}
