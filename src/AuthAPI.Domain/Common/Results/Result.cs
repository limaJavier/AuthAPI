namespace AuthAPI.Domain.Common.Results;

public sealed class Result : ResultBase
{
    private Result()
    {
        IsSuccess = true;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _error = error;
    }

    public static Result Success() => new();
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => new(error);
}

public sealed class Result<T> : ResultBase
{
    private readonly T _value;

    public T Value =>
        IsSuccess 
            ? _value! 
            : throw new InvalidOperationException("Cannot access Value on failure.");

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _error = error;
        _value = default!;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
}
