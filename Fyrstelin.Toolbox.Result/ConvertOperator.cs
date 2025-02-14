namespace Fyrstelin.Toolbox;

public static class ConvertOperator
{
    public static Result<TResult, TError> Convert<T1, T2, TResult, TError>(
        this Result<(T1, T2), TError> that,
        Func<T1, T2, TResult> converter
    ) => that
        .Convert(t => converter(t.Item1, t.Item2));

}