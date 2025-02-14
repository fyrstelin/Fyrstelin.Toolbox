namespace Fyrstelin.Toolbox.Tests;

public abstract class ResultTesting
{
    protected static Result<T, Error> Ok<T>(T value) => value;
    protected static Result<T, Error> Fail<T>(Error value) => value;

    public record Error(string Message);
    public record AnotherError(string Message);
}