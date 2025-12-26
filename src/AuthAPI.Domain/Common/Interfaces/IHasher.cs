namespace AuthAPI.Domain.Common.Interfaces;

public interface IHasher
{
    string Hash(string text);
    bool Verify(string text, string hash);
}
