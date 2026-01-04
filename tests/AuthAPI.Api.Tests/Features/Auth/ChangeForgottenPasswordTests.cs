using System.Net;
using AuthAPI.Api.Features.Auth.ChangeForgottenPassword;
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

public class ChangeForgottenPasswordTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : BaseAuthTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenRecoveryCodeIsVerifiedAndPasswordIsValid_ShouldResetPassword()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var (verificationToken, verificationCode) = await _authFlows.ForgotPasswordAsync(); // Forgot password

        await _authFlows.VerifyRecoveryCodeAsync(verificationToken, verificationCode); // Verify recovery code

        var request = new ChangeForgottenPasswordRequest(verificationToken, "New" + Constants.User.Password);

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();
        var verificationSessionManager = _serviceProvider.GetRequiredService<IVerificationSessionManager>();

        //** Act
        await _client.SendAndEnsureSuccessAsync(HttpMethod.Post, Routes.Auth.ChangeForgottenPassword, request);

        var user = (await _dbContext.Users
            .Include(user => user.RefreshTokens)
            .FirstOrDefaultAsync(user => user.Email == Constants.User.Email))!;

        var session = await verificationSessionManager.GetSessionAsync(verificationToken);

        //** Assert
        Assert.True(hasher.Verify(request.Password, user.PasswordHash!)); // Verify new password
        Assert.All(user.RefreshTokens, token => Assert.NotNull(token.RevokedAtUtc)); // Verify refresh tokens were revoked
        Assert.Null(session); // Session was removed
    }

    [Fact]
    public async Task WhenRecoveryCodeIsNotVerified_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var (verificationToken, _) = await _authFlows.ForgotPasswordAsync(); // Forgot password

        var request = new ChangeForgottenPasswordRequest(verificationToken, "New" + Constants.User.Password);

        //** Act
        var httpResponse = await _client.SendAsync(HttpMethod.Post, Routes.Auth.ChangeForgottenPassword, request);

        //** Assert
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact]
    public async Task WhenVerificationTokenIsInvalid_ShouldReturnConflict()
    {
        //** Arrange
        await _authFlows.RegisterAndVerifyAsync(); // Register and verify user

        var request = new ChangeForgottenPasswordRequest("WrongVerificationToken", "New" + Constants.User.Password);

        //** Act
        var httpResponse = await _client.SendAsync(HttpMethod.Post, Routes.Auth.ChangeForgottenPassword, request);

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Theory]
    [MemberData(nameof(BadChangeForgottenPasswordRequests))]
    public async Task WhenRequestIsBad_ShouldReturnBadRequest(ChangeForgottenPasswordRequest request)
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

    public static IEnumerable<object[]> BadChangeForgottenPasswordRequests()
    {
        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "", Password: Constants.User.Password)];

        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "Token", Password: "short1!")];
        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "Token", Password: "alllowercase1!")];
        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "Token", Password: "ALLUPPERCASE1!")];
        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "Token", Password: "NoDigits!")];
        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "Token", Password: "NoSpecialChar1")];
        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "Token", Password: new string('a', 256) + "A1!")];
    }
}