namespace AuthAPI.Domain.Common.Results;

public record Error(
    string Message,
    ErrorType Type
)
{
    public static Error Unexpected(string message) => new(message, ErrorType.Unexpected);
    public static Error Validation(string message) => new(message, ErrorType.Validation);
    public static Error Conflict(string message) => new(message, ErrorType.Conflict);
    public static Error NotFound(string message) => new(message, ErrorType.NotFound);
    public static Error Unauthorized(string message) => new(message, ErrorType.Unauthorized);
    public static Error Forbidden(string message) => new(message, ErrorType.Forbidden);
}

public enum ErrorType
{
    Unexpected,
    Validation,
    Conflict,
    NotFound,
    Unauthorized,
    Forbidden,
}
