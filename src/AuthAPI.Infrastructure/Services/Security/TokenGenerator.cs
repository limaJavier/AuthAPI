using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthAPI.Domain.Common.Interfaces;
using AuthAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Infrastructure.Services.Security;

public class TokenGenerator(IOptions<JwtSettings> jwtSettingsOptions) : ITokenGenerator
{
    private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;

    public string GenerateAccessToken(AccessTokenGenerationParameters generationParameters)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)); // Build key
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Build credentials from key

        // Define claims
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token's id
            new Claim(JwtRegisteredClaimNames.Sub, generationParameters.UserId.ToString()), // User's id
            new Claim(JwtRegisteredClaimNames.Email, generationParameters.Email), // User's email
        };

        // Build token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRandomToken(int size = 64)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(size));
    }
}
