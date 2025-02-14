using Microsoft.VisualBasic.CompilerServices;

namespace Fyrstelin.Toolbox.Linq;

public static partial class Operators
{
    public static Result<TOut, TError> Select<TIn, TOut, TError>(
        this Result<TIn, TError> that,
        Func<TIn, TOut> selector
    ) => that.Convert(selector);

    public static Result<TOut, TError> Select<TIn, TOut, TError>(
        this Result<TIn, TError> that,
        Func<TIn, Result<TOut, TError>> selector
    ) => that.Convert(selector);

    public static Task<Result<TOut, TError>> Select<TIn, TOut, TError>(
        this Task<Result<TIn, TError>> that,
        Func<TIn, TOut> selector
    ) => that
        .ContinueWith(result => result.Result.Convert(selector));
}