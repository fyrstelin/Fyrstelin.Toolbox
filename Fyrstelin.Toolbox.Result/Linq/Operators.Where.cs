namespace Fyrstelin.Toolbox.Linq;

public static partial class Operators
{
    public static Result<T, TError> Where<T, TTemp, TError>(
        this Result<T, TError> result,
        Func<T, Result<TTemp, TError>> predicate
    ) => result.Convert(x => predicate(x).Convert(_ => x));
    public static Task<Result<T, TError>> Where<T, TTemp, TError>(
        this Task<Result<T, TError>> result,
        Func<T, Result<TTemp, TError>> predicate
    ) => result
        .ContinueWith(task => task.Result.Convert(x => predicate(x).Convert(_ => x)));
    public static Task<Result<T, TError>> Where<T, TTemp, TError>(
        this Result<T, TError> result,
        Func<T, Task<Result<TTemp, TError>>> predicate
    ) => result
        .ConvertAsync(x => predicate(x)
            .ContinueWith(task => task.Result.Convert(_ => x)));
    
    public static Task<Result<T, TError>> Where<T, TTemp, TError>(
        this Task<Result<T, TError>> result,
        Func<T, Task<Result<TTemp, TError>>> predicate
    ) => result
        .ContinueWith(t1 => t1.Result
            .ConvertAsync(x => predicate(x)
                .ContinueWith(task => task.Result.Convert(_ => x))))
        .Unwrap();
}