using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Domain.Common.Results;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace AuthAPI.Infrastructure.Services.Security;

public class GoogleValidator(IConfiguration configuration) : IGoogleValidator
{
    private readonly string _clientId = configuration["Authentication:Google:ClientId"] ?? throw new Exception("Cannot resolve Google ClientId");

    public async Task<Result<GooglePayload>> ValidateAsync(string token)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [_clientId]
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
        }
        catch (Exception e)
        {
            return Error.Unauthorized(e.Message);
        }

        if (!payload.EmailVerified)
            return Error.Unauthorized("Google's email is not verified");

        return new GooglePayload(
            Identifier: payload.Subject,
            Email: payload.Email,
            Name: payload.Name
        );
    }
}
