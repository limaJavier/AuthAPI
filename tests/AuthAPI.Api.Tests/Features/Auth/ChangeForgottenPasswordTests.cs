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
    public async Task WhenRecoveryCodeIsVerifiedAndPasswordIsValid_ShouldResetPasswordAndCloseAllSessions()
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

        var user = await _dbContext.Users
            .Include(user => user.Sessions)
                .ThenInclude(session => session.RefreshTokens)
            .FirstAsync(user => user.Email == Constants.User.Email);

        var verificationSession = await verificationSessionManager.GetSessionAsync(verificationToken);

        //** Assert
        Assert.True(hasher.Verify(request.Password, user.PasswordHash!)); // Verify new password
        Assert.All(user.Sessions, session =>
        {
            Assert.NotNull(session.ClosedAtUtc);
            Assert.Null(session.CurrentRefreshToken);
        }); // All sessions were closed
        Assert.Null(verificationSession); // Verification-session was removed
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
    public Task WhenRequestIsBad_ShouldReturnBadRequest(ChangeForgottenPasswordRequest request) =>
        CommonTestMethods.WhenRequestIsBad_ShouldReturnBadRequest(
            _client,
            HttpMethod.Post,
            Routes.Auth.ChangeForgottenPassword,
            request
        );

    public static IEnumerable<object[]> BadChangeForgottenPasswordRequests()
    {
        yield return [new ChangeForgottenPasswordRequest(VerificationToken: "", Password: Constants.User.Password)];
        foreach (var badPassword in Constants.User.BadPasswords)
            yield return [new ChangeForgottenPasswordRequest(VerificationToken: "VerificationToken", Password: badPassword)];
    }
}
