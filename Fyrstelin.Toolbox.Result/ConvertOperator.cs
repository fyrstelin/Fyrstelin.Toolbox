namespace Fyrstelin.Toolbox;

public static class ConvertOperator
{
    public static Result<TOut, TError> Convert<TIn, TOut, TError>(
        this Result<TIn, TError> that,
        Func<TIn, TOut> converter
    ) => that
        .Convert<TOut, TError>(
            x => converter(x),
            err => err
        );

    public static Result<TOut, TError> Convert<TIn, TOut, TError>(
        this Result<TIn, TError> that,
        Func<TIn, Result<TOut, TError>> converter
    ) => that
        .Convert(
            converter,
            err => err
        );

    public static Task<Result<TOut, TError>> ConvertAsync<TIn, TOut, TError>(
        this Result<TIn, TError> that,
        Func<TIn, Task<TOut>> converter
    ) => that
        .ConvertAsync(
            x => converter(x).ContinueWith<Result<TOut, TError>>(t => t.Result),
            err => Task.FromResult<Result<TOut, TError>>(err)
        );

    public static Task<Result<TOut, TError>> ConvertAsync<TIn, TOut, TError>(
        this Result<TIn, TError> that,
        Func<TIn, Task<Result<TOut, TError>>> converter
    ) => that
        .ConvertAsync(
            converter,
            err => Task.FromResult<Result<TOut, TError>>(err)
        );

    public static Result<TResult, TError> Convert<T1, T2, TResult, TError>(
        this Result<(T1, T2), TError> that,
        Func<T1, T2, TResult> converter
    ) => that
        .Convert(t => converter(t.Item1, t.Item2));
}
