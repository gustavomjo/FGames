namespace FGames.SharedKernel;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyCollection<Error> Errors { get; }

    protected Result(bool isSuccess, IReadOnlyCollection<Error> errors)
    {
        if (isSuccess && errors.Count > 0)
            throw new InvalidOperationException("A successful result cannot contain errors.");

        if (!isSuccess && errors.Count == 0)
            throw new InvalidOperationException("A failed result must contain at least one error.");

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public Error FirstError => Errors.FirstOrDefault() ?? Error.None;

    public static Result Success() => new(true, []);

    public static Result Failure(Error error) => new(false, [error]);

    public static Result Failure(IReadOnlyCollection<Error> errors) => new(false, errors);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, []);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, [error]);

    public static Result<TValue> Failure<TValue>(IReadOnlyCollection<Error> errors) => new(default, false, errors);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, IReadOnlyCollection<Error> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
