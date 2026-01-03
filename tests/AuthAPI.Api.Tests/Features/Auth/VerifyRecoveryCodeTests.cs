using AuthAPI.Api.Features.Auth.VerifyRecoveryCode;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Api.Tests.Features.Utils;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using AuthAPI.Application.Common.Interfaces;
using System.Net;

namespace AuthAPI.Api.Tests.Features.Auth;

public class VerifyRecoveryCodeTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenRequestIsValid_ShouldVerifySession()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify
        var (verificationToken, verificationCode) = await _authFlows.ForgotPasswordAsync(); // Send forgot password request

        var request = new VerifyRecoveryCodeRequest(verificationToken, verificationCode);

        // Resolve dependencies
        var verificationSessionManager = _serviceProvider.GetRequiredService<IVerificationSessionManager>();

        //** Act
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.VerifyRecoveryCode,
            body: request
        );

        var session = (await verificationSessionManager.GetSessionAsync(verificationToken))!;

        //** Assert
        Assert.True(session.IsVerified);
    }

    [Fact]
    public async Task WhenVerificationCodeIsWrong_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify
        var (verificationToken, _) = await _authFlows.ForgotPasswordAsync(); // Send forgot password request

        var request = new VerifyRecoveryCodeRequest(verificationToken, "WrongVerificationCode");

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.VerifyRecoveryCode,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenVerificationTokenDoesNotExist_ShouldReturnUnauthorized()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify
        var (_, verificationCode) = await _authFlows.ForgotPasswordAsync(); // Send forgot password request

        var request = new VerifyRecoveryCodeRequest("WrongVerificationCode", verificationCode);

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.VerifyRecoveryCode,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenSessionIsAlredyVerified_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify
        var (verificationToken, verificationCode) = await _authFlows.ForgotPasswordAsync(); // Send forgot password request

        var request = new VerifyRecoveryCodeRequest(verificationToken, verificationCode);

        //** Act
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.VerifyRecoveryCode,
            body: request
        );

        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.VerifyRecoveryCode,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }
}