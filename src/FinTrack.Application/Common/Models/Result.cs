namespace FinTrack.Application.Common.Models;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail for
/// an expected business reason, without resorting to exceptions for
/// everyday failures. Exceptions are reserved for truly exceptional,
/// unexpected situations (a domain invariant violated, infrastructure
/// failure) — not for "the user's budget amount was negative."
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot have an error message.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failed result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Generic version carrying an actual return value on success.
/// E.g. a query returning a TransactionDto, or a command returning
/// the new entity's Id.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}