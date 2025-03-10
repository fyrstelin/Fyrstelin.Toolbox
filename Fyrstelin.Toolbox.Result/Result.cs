using System.Collections;

namespace Fyrstelin.Toolbox;

public readonly struct Result<TValue, TError>
{
    private readonly bool _isSuccess;
    private readonly TValue _value;
    private readonly TError _error;

    private Result(TValue value)
    {
        _isSuccess = true;
        _value = value;
        _error = default!;
    }

    private Result(TError error)
    {
        _isSuccess = false;
        _error = error;
        _value = default!;
    }

    public Result<TV, TE> Convert<TV, TE>(
        Func<TValue, Result<TV, TE>> onSuccess,
        Func<TError, Result<TV, TE>> onFailure
    ) => _isSuccess ? onSuccess(_value) : onFailure(_error);

    public Task<Result<TV, TE>> ConvertAsync<TV, TE>(
        Func<TValue, Task<Result<TV, TE>>> onSuccess,
        Func<TError, Task<Result<TV, TE>>> onFailure
    ) => _isSuccess ? onSuccess(_value) : onFailure(_error);

    public T Match<T>(Func<TValue, T> onSuccess, Func<TError, T> onError) =>
        _isSuccess ? onSuccess(_value) : onError(_error);

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);
    
    // Unit and never
    public static implicit operator Result<TValue, TError>(Result<TValue, Never> result) => new(result._value);
    public static implicit operator Result<Unit, TError>(Result<TValue, TError> result) => result.Convert<Unit, TError>(
        _ => Result.Ok(),
        e => e
    );

    public static implicit operator bool(Result<TValue, TError> value) => value._isSuccess;

    public override string ToString() => _isSuccess
        ? $"Success<{typeof(TValue).Name}, {typeof(TError).Name}>({_value})"
        : $"Failure<{typeof(TValue).Name}, {typeof(TError).Name}>({_error})";
}

public static class Result
{
    private static readonly Result<Unit, Never> OkUnit = new Unit();
    public static Result<Unit, Never> Ok() => OkUnit;
    public static Result<T, Never> Ok<T>(T value) => value;

    public static Result<(T1, T2), TError> Combine<T1, T2, TError>(
        Result<T1, TError> r1,
        Result<T2, TError> r2
    )
    {
        var errors = new ErrorCollection<TError>();

        var v1 = errors.Resolve(r1);
        var v2 = errors.Resolve(r2);

        return errors.Any()
            ? errors.First()
            : (v1, v2);
    }

    private class ErrorCollection<TError> : IReadOnlyCollection<TError>
    {
        private readonly List<TError> _errors = [];

        public IEnumerator<TError> GetEnumerator() => _errors.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _errors.GetEnumerator();

        public int Count => _errors.Count;

        public T Resolve<T>(Result<T, TError> result) => result.Match(
            x => x,
            err =>
            {
                _errors.Add(err);
                return default!;
            }
        );
    }
}