namespace Fyrstelin.Toolbox.Linq;

public static class SelectManyOperator
{
    public static Result<TTarget, TError> SelectMany<TSource, TTemp, TTarget, TError>(
        this Result<TSource, TError> that,
        Func<TSource, Result<TTemp, TError>> selector,
        Func<TSource, TTemp, TTarget> combine
    ) => that
        .Convert(x => selector(x).Convert(y => combine(x, y)));
    
    public static Task<Result<TTarget, TError>> SelectMany<TSource, TTemp, TTarget, TError>(
        this Task<Result<TSource, TError>> that,
        Func<TSource, Result<TTemp, TError>> selector,
        Func<TSource, TTemp, TTarget> combine
    ) => that
        .ContinueWith(task => task
            .Result
            .Convert(x => selector(x).Convert(y => combine(x, y))));

    public static Task<Result<TTarget, TError>> SelectMany<TSource, TTemp, TTarget, TError>(
        this Result<TSource, TError> that,
        Func<TSource, Task<Result<TTemp, TError>>> selector,
        Func<TSource, TTemp, TTarget> combine
    ) => that
        .ConvertAsync(x => selector(x)
            .ContinueWith(task => task.Result.Convert(y => combine(x, y))));

    public static Task<Result<TTarget, TError>> SelectMany<TSource, TTemp, TTarget, TError>(
        this Task<Result<TSource, TError>> that,
        Func<TSource, Task<Result<TTemp, TError>>> selector,
        Func<TSource, TTemp, TTarget> combine
    ) => that
            .ContinueWith(t1 => t1.Result
                .ConvertAsync(x => selector(x)
                    .ContinueWith(t2 => t2.Result.Convert(y => combine(x, y)))))
            .Unwrap();
    
}