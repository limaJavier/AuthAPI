using System.Net.Http.Json;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.RegisterWithEmail;
using AuthAPI.Api.Features.Auth.VerifyEmail;
using AuthAPI.Api.Tests.Features.Utils;
using AuthAPI.Api.Tests.Features.Utils.Constants;
using AuthAPI.Api.Tests.Features.Utils.Routes;
using AuthAPI.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.Api.Tests.Features.Common.Flows;

public class AuthFlows(IServiceProvider serviceProvider, HttpClient client) : AbstractFlows(serviceProvider, client)
{
    public async Task<(string VerificationToken, string VerificationCode)> RegisterAsync(
        string name = Constants.User.Name,
        string email = Constants.User.Email,
        string password = Constants.User.Password
    )
    {
        var registerRequest = new RegisterWithEmailRequest(
            name,
            email,
            password
        );

        var response = await _client.SendAsync<VerificationResponse>(
            HttpMethod.Post,
            Routes.Auth.Register,
            registerRequest
        );

        var verificationCode = await GetVerificationCodeAsync(response.VerificationToken);

        return (response.VerificationToken, verificationCode);
    }

    public async Task<(string AccessToken, string RefreshToken)> VerifyEmailAsync(string verificationToken, string verificationCode)
    {
        var verificationRequest = new VerifyEmailRequest(
            verificationToken,
            verificationCode
        );

        var httpResponse = await _client.SendAndEnsureSuccessAsync(HttpMethod.Post, Routes.Auth.VerifyEmail, verificationRequest);

        var refreshTokenHeader = httpResponse.Headers.GetValues("Set-Cookie").First();
        var refreshToken = ExtractTokenFromCookie(refreshTokenHeader);

        var response = (await httpResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        return (response.AccessToken, refreshToken);
    }

    public async Task<(string AccessToken, string RefreshToken)> RegisterAndVerifyAsync(
        string name = Constants.User.Name,
        string email = Constants.User.Email,
        string password = Constants.User.Password
    )
    {
        var (verificationToken, verificationCode) = await RegisterAsync(name, email, password); // Register user
        return await VerifyEmailAsync(verificationToken, verificationCode); // Verify user
    }

    public static string ExtractTokenFromCookie(string cookie, string tokenName = "refresh_token")
    {
        var parts = cookie.Split(';');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith(tokenName + "=", StringComparison.OrdinalIgnoreCase))
            {
                var value = trimmed.Substring(tokenName.Length + 1);
                return Uri.UnescapeDataString(value);
            }
        }
        return string.Empty;
    }

    private async Task<string> GetVerificationCodeAsync(string verificationToken)
    {
        var verificationSessionManager = _serviceProvider.GetRequiredService<IVerificationSessionManager>();
        var session = (await verificationSessionManager.GetSessionAsync(verificationToken))!;
        return session.Code;
    }
}