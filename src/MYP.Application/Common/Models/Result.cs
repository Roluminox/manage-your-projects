namespace MYP.Application.Common.Models;

public class Result
{
    protected Result(bool isSuccess, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors?.ToArray() ?? Array.Empty<string>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string[] Errors { get; }

    public static Result Success() => new(true);
    public static Result Failure(params string[] errors) => new(false, errors);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(params string[] errors) => Result<T>.Failure(errors);
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true)
    {
        _value = value;
    }

    private Result(IEnumerable<string> errors) : base(false, errors)
    {
        _value = default;
    }

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access Value on a failed result.");
            return _value!;
        }
    }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(params string[] errors) => new(errors);
    public new static Result<T> Failure(IEnumerable<string> errors) => new(errors);

    public static implicit operator Result<T>(T value) => Success(value);
}
