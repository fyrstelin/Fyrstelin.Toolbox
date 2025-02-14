using Shouldly;

namespace Fyrstelin.Toolbox.Tests.Results;

public class ResultExtensionsTests : Tests
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
}