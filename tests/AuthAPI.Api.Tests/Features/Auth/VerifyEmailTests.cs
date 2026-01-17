using System.Net;
using System.Net.Http.Json;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.VerifyEmail;
using AuthAPI.Api.Tests.Features.Common.Flows;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class VerifyEmailTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenVerificationTokenAndCodeAreValid_ShouldCreateASessionAndReturnAccessAndRefreshTokens()
    {
        //** Arrange
        var (verificationToken, verificationCode) = await _authFlows.RegisterAsync(); // Register user

        var request = new VerifyEmailRequest(
            verificationToken,
            verificationCode
        );

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();
        var verificationSessionManager = _serviceProvider.GetRequiredService<IVerificationSessionManager>();

        //** Act
        var httpResponse = await _client.SendAndEnsureSuccessAsync(HttpMethod.Post, Routes.Auth.VerifyEmail, request);

        var refreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var refreshTokenStr = AuthFlows.ExtractTokenFromCookie(refreshTokenHeader);
        var response = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        var user = await _dbContext.Users
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        var verificationSession = await verificationSessionManager.GetSessionAsync(verificationToken);

        //** Assert
        Assert.NotNull(refreshTokenHeader);
        Assert.NotEmpty(refreshTokenHeader);
        Assert.NotEmpty(response.AccessToken);
        Assert.True(user.IsVerified); // User verified
        Assert.Contains(user.Sessions, session => hasher.VerifyDeterministic(refreshTokenStr, session.CurrentRefreshToken!.Hash));
        Assert.Null(verificationSession); // Verification-session was removed
    }

    [Fact]
    public async Task WhenEmailWasAlreadyVerified_ShouldReturnUnauthorized()
    {
        //** Arrange
        var (verificationToken, verificationCode) = await _authFlows.RegisterAsync(); // Register user

        var request = new VerifyEmailRequest(
            verificationToken,
            verificationCode
        );

        //** Act
        await _client.SendAndEnsureSuccessAsync(HttpMethod.Post, Routes.Auth.VerifyEmail, request); // Verify email
        var httpResponse = await _client.SendAsync(HttpMethod.Post, Routes.Auth.VerifyEmail, request); // Verify again

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenVerificationTokenIsInvalid_ShouldReturnUnauthorized()
    {
        //** Arrange
        var (_, verificationCode) = await _authFlows.RegisterAsync(); // Register user

        var request = new VerifyEmailRequest(
            "WrongVerificationToken",
            verificationCode
        );

        //** Act
        var httpResponse = await _client.SendAsync(HttpMethod.Post, Routes.Auth.VerifyEmail, request);

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenVerificationCodeIsInvalid_ShouldReturnConflict()
    {
        //** Arrange
        var (verificationToken, _) = await _authFlows.RegisterAsync(); // Register user

        var request = new VerifyEmailRequest(
            verificationToken,
            "WrongVerificationCode"
        );

        //** Act
        var httpResponse = await _client.SendAsync(HttpMethod.Post, Routes.Auth.VerifyEmail, request);

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }
}
