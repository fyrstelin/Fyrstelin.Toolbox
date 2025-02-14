namespace Fyrstelin.Toolbox.Tests.Results;

public abstract class Tests
{
    protected static Result<T, Error> Ok<T>(T value) => value;
    protected static Result<T, Error> Fail<T>(Error value) => value;

    public record Error(string Message);
    public record AnotherError(string Message);
}