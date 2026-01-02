using AuthAPI.Domain.Common.Interfaces;

namespace AuthAPI.Infrastructure.Services.Security;

public class Hasher : IHasher
{
    public string Hash(string text)
    {
        return BCrypt.Net.BCrypt.HashPassword(text);
    }

    public bool Verify(string text, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(text, hash);
    }

    public string HashDeterministic(string text)
    {
        var hashedBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyDeterministic(string text, string hash)
    {
        return HashDeterministic(text) == hash;
    }
}
