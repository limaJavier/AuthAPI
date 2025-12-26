namespace AuthAPI.Application.Common.Interfaces;

public interface IVerificationSessionManager
{
    Task<VerificationSession?> GetSessionAsync(string token);
    Task<string> AddSessionAsync(string email);
    Task UpdateSession(VerificationSession session);
    Task RemoveSession(string token);
}
