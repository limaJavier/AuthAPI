using System.Net;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.RegisterWithEmail;
using AuthAPI.Api.Tests.Features.Common;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Api.Tests.Fixtures;
using AuthAPI.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthAPI.Api.Tests.Features.Auth;

public class RegisterWithEmailTests(ITestOutputHelper output, PostgresContainerFixture postgresContainerFixture) : IsolatedTests(output, postgresContainerFixture)
{
    [Fact]
    public async Task WhenEmailIsNotRegistered_ShouldCreateUserAndReturnVerificationToken()
    {
        //** Arrange
        var request = AuthRequestsFactory.CreateRegisterRequest();

        // Resolve dependencies
        var hasher = _serviceProvider.GetRequiredService<IHasher>();

        //** Act
        var response = await _client.SendAsync<VerificationResponse>(
            method: HttpMethod.Post,
            route: Routes.Auth.Register,
            body: request
        );

        var users = await _dbContext.Users
            .Where(user =>
                user.IsActive &&
                user.Email == request.Email)
            .ToListAsync();
        var user = users[0];

        //** Assert
        Assert.NotEmpty(response.VerificationToken);
        Assert.Single(users);
        Assert.Equal(request.Email, user.Email);
        Assert.NotNull(user.PasswordHash);
        Assert.True(hasher.Verify(request.Password, user.PasswordHash));
    }

    [Fact]
    public async Task WhenEmailIsAlreadyRegistered_ShouldReturnConflict()
    {
        //** Arrange
        var request1 = AuthRequestsFactory.CreateRegisterRequest();
        var request2 = AuthRequestsFactory.CreateRegisterRequest(name: "Jack");

        //** Act
        // Register the first user
        await _client.SendAndEnsureSuccessAsync(HttpMethod.Post, Routes.Auth.Register, request1);

        // Register the second user
        var httpResponse2 = await _client.SendAsync(HttpMethod.Post, Routes.Auth.Register, request2);

        // Retrieve active user with the request email from database
        var users = await _dbContext.Users.Where(user => user.Email == request1.Email).ToListAsync();
        var user = users[0];

        //** Assert
        Assert.Single(users);
        Assert.Equal(HttpStatusCode.Conflict, httpResponse2.StatusCode);
    }

    [Theory]
    [MemberData(nameof(BadRegisterWithEmailRequests))]
    public Task WhenRequestIsBad_ShouldReturnBadRequest(RegisterWithEmailRequest request) =>
        CommonTestMethods.WhenRequestIsBad_ShouldReturnBadRequest(
            _client,
            HttpMethod.Post,
            Routes.Auth.Register,
            request
        );

    public static IEnumerable<object[]> BadRegisterWithEmailRequests()
    {
        yield return [AuthRequestsFactory.CreateRegisterRequest(name: "")];

        foreach (var badEmail in Constants.User.BadEmails)
            yield return [AuthRequestsFactory.CreateRegisterRequest(email: badEmail)];

        foreach (var badPassword in Constants.User.BadPasswords)
            yield return [AuthRequestsFactory.CreateRegisterRequest(password: badPassword)];
    }
}