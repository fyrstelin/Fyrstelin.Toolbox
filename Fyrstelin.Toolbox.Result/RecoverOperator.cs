namespace Fyrstelin.Toolbox;

public static class UnwrapOperator
{
    public static T Unwrap<T>(this Result<T, Never> that) =>
        that.Match(x => x, _ => throw new Exception($"Unexpected error in {nameof(Unwrap)}"));
}

public static class RecoverOperator
{
    public static Result<T, Never> Recover<T, TError>(
        this Result<T, TError> that,
        Func<TError, T> recover
    ) => that.Convert<T, Never>(
        v => v,
        e => recover(e)
    );
    public static Result<T, TErrorOut> Recover<T, TErrorIn, TErrorOut>(
        this Result<T, TErrorIn> that,
        Func<TErrorIn, Result<T, TErrorOut>> recover
    ) => that.Convert(
        v => v,
        recover
    );
}