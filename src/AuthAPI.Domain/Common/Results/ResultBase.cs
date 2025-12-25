namespace AuthAPI.Domain.Common.Results;

public abstract class ResultBase
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;

    protected Error? _error;

    public Error Error =>
        IsFailure
            ? _error ?? throw new InvalidOperationException("Error was not set.")
            : throw new InvalidOperationException("Cannot access Error on success.");
}
