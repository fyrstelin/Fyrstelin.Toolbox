using Shouldly;

namespace Fyrstelin.Toolbox.Tests;

public class ResultExtensionsTests : ResultTesting
{
    [Fact]
    public void ShouldConvertToResult()
    {
        Ok("hello")
            .Convert(x => Ok(x.ToUpper()))
            .ShouldBe("HELLO");
    }

    [Fact]
    public void ShouldConvertToValue()
    {
        Ok("hello")
            .Convert(x => x.ToUpper())
            .ShouldBe("HELLO");
    }

    [Fact]
    public void ShouldConvertToError()
    {
        Ok("hello")
            .Convert(x => Fail<int>(new Error(x)))
            .ShouldBe(new Error("hello"));
    }

    [Fact]
    public async Task ShouldConvertAsyncToResult()
    {
        var result = await Ok("hello")
            .ConvertAsync(x => Task.FromResult(Ok(x.ToUpper())));

        result.ShouldBe("HELLO");
    }

    [Fact]
    public async Task ShouldConvertAsyncToValue()
    {
        var result = await Ok("hello")
            .ConvertAsync(x => Task.FromResult(x.ToUpper()));

        result.ShouldBe("HELLO");
    }

    [Fact]
    public async Task ShouldConvertAsyncToError()
    {
        var result = await Ok("hello")
            .ConvertAsync(x => Task.FromResult(Fail<int>(new Error(x))));

        result
            .ShouldBe(new Error("hello"));
    }

    [Fact]
    public void ShouldRecoverWithValue()
    {
        var result = Fail<string>(new Error("sad"));

        result
            .Recover(err => "I was " + err.Message + ". Now I am happy!!")
            .ShouldBeOfType<Result<string, Never>>()
            .ShouldBe("I was sad. Now I am happy!!");
    }

    [Fact]
    public void ShouldRecoverWithResult()
    {
        var result = Fail<string>(new Error("sad"));

        result
            .Recover(err => Ok("I was " + err.Message + ". Now I am happy!!"))
            .ShouldBeOfType<Result<string, Error>>()
            .ShouldBe("I was sad. Now I am happy!!");
    }

    [Fact]
    public void ShouldGetValue()
    {
        Result
            .Ok("Some value")
            .Unwrap()
            .ShouldBe("Some value");
    }
}