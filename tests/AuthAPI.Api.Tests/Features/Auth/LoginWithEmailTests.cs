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
    public async Task WhenUserWithEmailExistsIsVerifiedAndPasswordMatch_ShouldReturnAccessAndRefreshTokens()
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
        var loginResponse = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        var user = (await _dbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Email == Constants.User.Email))!;

        //** Assert
        Assert.NotNull(refreshTokenHeader);
        Assert.NotEmpty(refreshTokenHeader);
        Assert.NotEmpty(loginResponse.AccessToken);
        Assert.Contains(user.RefreshTokens, token => hasher.Verify(refreshTokenStr, token.Hash));
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
    public async Task WhenRequestIsBad_ShouldReturnBadRequest(LoginWithEmailRequest request)
    {
        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.Register,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    public static IEnumerable<object[]> BadLoginWithEmailRequests()
    {
        yield return [AuthRequestsFactory.CreateLoginRequest(email: "")];
        yield return [AuthRequestsFactory.CreateLoginRequest(password: "")];

        yield return [AuthRequestsFactory.CreateLoginRequest(email: "missingatsign.com")];
        yield return [AuthRequestsFactory.CreateLoginRequest(email: "missingdomain@")];
        yield return [AuthRequestsFactory.CreateLoginRequest(email: "@missingusername.com")];
        yield return [AuthRequestsFactory.CreateLoginRequest(email: "toolong" + new string('a', 250) + "@example.com")];

        yield return [AuthRequestsFactory.CreateLoginRequest(password: "short1!")];
        yield return [AuthRequestsFactory.CreateLoginRequest(password: "alllowercase1!")];
        yield return [AuthRequestsFactory.CreateLoginRequest(password: "ALLUPPERCASE1!")];
        yield return [AuthRequestsFactory.CreateLoginRequest(password: "NoDigits!")];
        yield return [AuthRequestsFactory.CreateLoginRequest(password: "NoSpecialChar1")];
        yield return [AuthRequestsFactory.CreateLoginRequest(password: new string('a', 256) + "A1!")];
    }
}