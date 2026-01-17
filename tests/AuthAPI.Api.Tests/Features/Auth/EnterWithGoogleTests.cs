using System.Net;
using System.Net.Http.Json;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.EnterWithGoogle;
using AuthAPI.Api.Tests.Features.Common.Flows;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class EnterWithGoogleTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenGoogleTokenIsValidAndUserIsNotRegistered_ShouldCreateTheUserCreateASessionAndReturnAccessAndRefreshTokens()
    {
        //** Arrange
        var request = new EnterWithGoogleRequest(Constants.User.GoogleToken);

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var httpResponse = await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.EnterWithGoogle,
            body: request
        );

        var refreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var refreshTokenStr = AuthFlows.ExtractTokenFromCookie(refreshTokenHeader);
        var response = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        var user = await _dbContext.Users
            .Include(user => user.Credentials)
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        //** Assert
        Assert.NotNull(refreshTokenHeader);
        Assert.NotEmpty(refreshTokenHeader);
        Assert.NotEmpty(response.AccessToken);
        Assert.Contains(user.Credentials, credential =>
            credential.Type == Domain.UserAggregate.Enums.CredentialType.Google &&
            credential.Identifier == Constants.User.GoogleIdentifier
        );
        Assert.Contains(user.Sessions, session => hasher.VerifyDeterministic(refreshTokenStr, session.CurrentRefreshToken!.Hash));
    }

    [Fact]
    public async Task WhenGoogleTokenIsValidAndUserWasRegisteredWithEmail_ShouldAddGoogleCredentialCreateASessionAndReturnAccessAndRefreshTokens()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user with email

        var request = new EnterWithGoogleRequest(Constants.User.GoogleToken);

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var httpResponse = await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.EnterWithGoogle,
            body: request
        );

        var refreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var refreshTokenStr = AuthFlows.ExtractTokenFromCookie(refreshTokenHeader);
        var response = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        var user = await _dbContext.Users
            .Include(user => user.Credentials)
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        //** Assert
        Assert.NotNull(refreshTokenHeader);
        Assert.NotEmpty(refreshTokenHeader);
        Assert.NotEmpty(response.AccessToken);
        Assert.Contains(user.Credentials, credential =>
            credential.Type == Domain.UserAggregate.Enums.CredentialType.Google &&
            credential.Identifier == Constants.User.GoogleIdentifier
        );
        Assert.Contains(user.Sessions, session => hasher.VerifyDeterministic(refreshTokenStr, session.CurrentRefreshToken!.Hash));
    }

    [Fact]
    public async Task WhenGoogleTokenIsValidAndUserWasRegisteredWithGoogle_ShouldCreateASessionAndReturnAccessAndRefreshTokens()
    {
        //** Arrange
        var request = new EnterWithGoogleRequest(Constants.User.GoogleToken);

        // Register user with google
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.EnterWithGoogle,
            body: request
        );

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var httpResponse = await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.EnterWithGoogle,
            body: request
        );

        var refreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var refreshTokenStr = AuthFlows.ExtractTokenFromCookie(refreshTokenHeader);
        var response = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        var user = await _dbContext.Users
            .Include(user => user.Credentials)
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        //** Assert
        Assert.NotNull(refreshTokenHeader);
        Assert.NotEmpty(refreshTokenHeader);
        Assert.NotEmpty(response.AccessToken);
        Assert.Contains(user.Credentials, credential =>
            credential.Type == Domain.UserAggregate.Enums.CredentialType.Google &&
            credential.Identifier == Constants.User.GoogleIdentifier
        );
        Assert.Contains(user.Sessions, session => hasher.VerifyDeterministic(refreshTokenStr, session.CurrentRefreshToken!.Hash));
    }

    [Fact]
    public async Task WhenUserIsNotVerified_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAsync(); // Register with email

        var request = new EnterWithGoogleRequest(Constants.User.GoogleToken);

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.EnterWithGoogle,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenUserWasRegisteredWithGoogleButIdentifierDoNotMatch_ShouldReturnUnauthorized()
    {
        //** Arrange
        var request = new EnterWithGoogleRequest(Constants.User.GoogleToken);

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();
        var googleValidator = _serviceProvider.GetRequiredService<IGoogleValidator>();
        var googleValidatorMock = Mock.Get(googleValidator);

        // Register user with google
        await _client.SendAndEnsureSuccessAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.EnterWithGoogle,
            body: request
        );

        // Configure mock to return a wrong identifier
        googleValidatorMock
            .Setup(validator => validator.ValidateAsync(Constants.User.GoogleToken))
            .ReturnsAsync(new GooglePayload(
                "Wrong" + Constants.User.GoogleIdentifier,
                Constants.User.Email,
                Constants.User.Name));

        //** Act
        var httpResponse = await _client.SendAsync(
            method: HttpMethod.Post,
            route: Routes.Auth.EnterWithGoogle,
            body: request
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }
}
