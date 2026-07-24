namespace Kompass.Application.Common;

public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
        {
            throw new ArgumentException("Ein erfolgreiches Result darf keinen Fehler enthalten.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Ein Fehler-Result benötigt eine Fehlermeldung.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public static Result Success()
        => new(true, null);

    public static Result Failure(string error)
        => new(false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(true, null)
    {
        _value = value;
    }

    private Result(string error)
        : base(false, error)
    {
    }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Ein Fehler-Result besitzt keinen Wert.");

    public static Result<T> Success(T value)
        => new(value);

    public new static Result<T> Failure(string error)
        => new(error);
}