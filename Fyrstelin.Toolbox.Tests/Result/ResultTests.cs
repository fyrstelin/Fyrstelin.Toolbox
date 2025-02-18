using Shouldly;

namespace Fyrstelin.Toolbox.Tests;

public class ResultTests : ResultTesting
{
    [Fact]
    public void ShouldCreateResultFromValue()
    {
        Result<string, Error> result = "Hello world";
        result
            .Match(
                s => s,
                err => err.Message
            )
            .ShouldBe("Hello world");
    }

    [Fact]
    public void ShouldCreateResultFromError()
    {
        Result<string, Error> result = new Error("ERROR");
        result
            .Match(
                s => s,
                err => err.Message
            )
            .ShouldBe("ERROR");
    }


    [Fact]
    public void ShouldToStringValue()
    {
        Ok("hello")
            .ToString()
            .ShouldBe("Success<String, Error>(hello)");
    }

    [Fact]
    public void ShouldToStringFailure()
    {
        Result<string, Error> failure = new Error("ERROR");

        failure
            .ToString()
            .ShouldBe("Failure<String, Error>(Error { Message = ERROR })");
    }

    [Fact]
    public void ShouldConvertValueToValue()
    {
        Ok("Hello")
            .Convert<int, Error>(
                value => value.Length,
                err => err
            )
            .ShouldBe(5);
    }

    [Fact]
    public void ShouldConvertValueToError()
    {
        Ok("Hello")
            .Convert<int, Error>(
                value => new Error(value),
                err => err
            )
            .ShouldBe(new Error("Hello"));
    }

    [Fact]
    public void ShouldConvertErrorToValue()
    {
        Fail<int>(new Error("ERROR"))
            .Convert<string, Error>(
                v => v.ToString(),
                e => e.Message
            )
            .ShouldBe("ERROR");
    }

    [Fact]
    public void ShouldConvertErrorToError()
    {
        Fail<string>(new Error("ERROR"))
            .Convert<string, AnotherError>(
                v => v,
                e => new AnotherError(e.Message)
            )
            .ShouldBe(new AnotherError("ERROR"));
    }

    [Fact]
    public async Task ShouldConvertAsyncValueToValue()
    {
        var result = await Ok("Hello")
            .ConvertAsync(
                value => Task.FromResult<Result<int, Error>>(value.Length),
                err => Task.FromResult<Result<int, Error>>(err)
            );

        result.ShouldBe(5);
    }

    [Fact]
    public async Task ShouldConvertAsyncValueToError()
    {
        var result = await Ok("Hello")
            .ConvertAsync(
                value => Task.FromResult<Result<int, Error>>(new Error(value)),
                err => Task.FromResult<Result<int, Error>>(err)
            );

        result.ShouldBe(new Error("Hello"));
    }

    [Fact]
    public async Task ShouldConvertAsyncErrorToValue()
    {
        var result = await Fail<int>(new Error("ERROR"))
            .ConvertAsync(
                v => Task.FromResult<Result<string, Error>>(v.ToString()),
                e => Task.FromResult<Result<string, Error>>(e.Message)
            );

        result.ShouldBe("ERROR");
    }

    [Fact]
    public async Task ShouldConvertAsyncErrorToError()
    {
        var result = await Fail<string>(new Error("ERROR"))
            .ConvertAsync(
                v => Task.FromResult<Result<string, AnotherError>>(v),
                e => Task.FromResult<Result<string, AnotherError>>(new AnotherError(e.Message))
            );

        result.ShouldBe(new AnotherError("ERROR"));
    }

    [Fact]
    public void ShouldConvertSuccessToTrue()
    {
        bool success = Ok("hello");
        success.ShouldBeTrue();
    }

    [Fact]
    public void ShouldConvertFailureToFalse()
    {
        bool success = Fail<string>(new Error("Eew"));
        success.ShouldBeFalse();
    }

    [Fact]
    public void ShouldCombine()
    {
        Result.Combine(Ok("hello"), Ok(42))
            .ShouldBe(("hello", 42));
    }

    [Fact]
    public void ShouldBeOk()
    {
        Result.Ok().ShouldBe(new Unit());
    }

    [Fact]
    public void ShouldBeOkWithValue()
    {
        Result.Ok("Hello").ShouldBe("Hello");
    }
    
    [Fact]
    public void ShouldNeverResultToAnyResult()
    {
        Result<string, Never> ok = "Hello";
        Result<string, Error> result = ok;

        result.ShouldBe("Hello");
    }

    [Fact]
    public void ShouldCastToUnitResult()
    {
        Result<string, Error> ok = "Hello";
        Result<Unit, Error> result = ok;
        result.ShouldBe(new Unit());
    }
}