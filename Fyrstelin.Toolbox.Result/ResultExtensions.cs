namespace Fyrstelin.Toolbox;

public static partial class ResultExtensions
{
    
}

public static partial class ResultExtensions
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
            x => converter(x).ContinueWith<Result<TOut, TError>> (t => t.Result),
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

}