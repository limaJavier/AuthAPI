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
}
