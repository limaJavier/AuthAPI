namespace AuthAPI.Domain.Common.Interfaces;

public interface IHasher
{
    string Hash(string text);
    bool Verify(string text, string hash);

    string HashDeterministic(string text);
    bool VerifyDeterministic(string text, string hash);
}
