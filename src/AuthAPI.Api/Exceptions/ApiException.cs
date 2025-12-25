using AuthAPI.Domain.Common.Results;

namespace AuthAPI.Api.Exceptions;

public class ApiException : Exception
{
    public ErrorType Type { get; private set; }
    public string Detail { get; private set; }

    private ApiException(
        string message,
        ErrorType type,
        string detail
    ) : base(message)
    {
        Type = type;
        Detail = detail;
    }

    public static ApiException Unexpected(string message = "", string detail = "") =>
        new(message, ErrorType.Unexpected, detail);

    public static ApiException Validation(string message = "", string detail = "") =>
        new(message, ErrorType.Validation, detail);

    public static ApiException Conflict(string message = "", string detail = "") =>
        new(message, ErrorType.Conflict, detail);

    public static ApiException NotFound(string message = "", string detail = "") =>
        new(message, ErrorType.NotFound, detail);

    public static ApiException Unauthorized(string message = "", string detail = "") =>
        new(message, ErrorType.Unauthorized, detail);

    public static ApiException Forbidden(string message = "", string detail = "") =>
        new(message, ErrorType.Forbidden, detail);

    public static ApiException FromError(Error error) =>
        new(error.Message, error.Type, "");
}
