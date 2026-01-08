using AuthAPI.Domain.Common.Results;

namespace AuthAPI.Application.Common;

public class VerificationSession(
    string token,
    string email,
    string code,
    DateTime expiresAtUtc
)
{
    public string Token {get; } = token;
    public string Email {get; } = email;
    public string Code {get; } = code;
    public DateTime ExpiresAtUtc {get; } = expiresAtUtc;
    public bool IsVerified { get; private set; } = false;

    public Result Verify(string code)
    {
        if (IsVerified)
            return Error.Conflict("Session is already verified");

        if (code != Code)
            return Error.Conflict("Verification-Code is wrong");

        IsVerified = true;
        return Result.Success();
    }
}
