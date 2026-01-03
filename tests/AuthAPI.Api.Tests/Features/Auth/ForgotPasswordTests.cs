using System.Net;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class ForgotPasswordTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenEmailIsValid_ShouldReturnVerificationCode()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync();

        var request = AuthRequestsFactory.CreateForgotPasswordRequest();

        //** Act
        var response = await _client.SendAsync<VerificationResponse>(
            method: HttpMethod.Post,
            route: Routes.Auth.ForgotPassword,
            body: request
        );

        //** Assert
        Assert.NotEmpty(response.VerificationToken);
    }

    [Fact]
    public async Task WhenEmailDoesNotExist_ShouldReturnNotFound()
    {
        //** Arrange
        var request = AuthRequestsFactory.CreateForgotPasswordRequest();

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.ForgotPassword,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenUserIsNotVerified_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAsync();

        var request = AuthRequestsFactory.CreateForgotPasswordRequest();

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.ForgotPassword,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }
}