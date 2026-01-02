using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.RegisterWithEmail;
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

    private async Task<string> GetVerificationCodeAsync(string verificationToken)
    {
        var verificationSessionManager = _serviceProvider.GetRequiredService<IVerificationSessionManager>();
        var session = (await verificationSessionManager.GetSessionAsync(verificationToken))!;
        return session.Code;
    }
}