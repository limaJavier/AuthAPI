using AuthAPI.Domain.Common.Results;

namespace AuthAPI.Application.Common.Interfaces;

public interface IGoogleValidator
{
    Task<Result<GooglePayload>> ValidateAsync(string token);
}

public record GooglePayload(
    string Identifier,
    string Email,
    string Name
);

