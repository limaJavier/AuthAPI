namespace AuthAPI.Domain.Common.Results;

public record Error(
    string Message,
    ErrorType Type,
    string? Detail = null
)
{
    public static Error Unexpected(string message, string? detail = null) => new(message, ErrorType.Unexpected, detail);
    public static Error Validation(string message, string? detail = null) => new(message, ErrorType.Validation, detail);
    public static Error Conflict(string message, string? detail = null) => new(message, ErrorType.Conflict, detail);
    public static Error NotFound(string message, string? detail = null) => new(message, ErrorType.NotFound, detail);
    public static Error Unauthorized(string message, string? detail = null) => new(message, ErrorType.Unauthorized, detail);
    public static Error Forbidden(string message, string? detail = null) => new(message, ErrorType.Forbidden, detail);
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
