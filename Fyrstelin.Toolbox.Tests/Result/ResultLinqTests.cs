using Fyrstelin.Toolbox.Linq;
using Shouldly;

namespace Fyrstelin.Toolbox.Tests;

public class ResultLinqTests : ResultTesting
{
    [Fact]
    public void ShouldSelect()
    {
        var res =
            from greeting in Ok("hello world")
            select greeting.ToUpper();


        res.ShouldBe("HELLO WORLD");
    }

    [Fact]
    public async Task ShouldSelectAsync()
    {
        var res = await (
            from greeting in Task.FromResult(Ok("hello world"))
            select greeting.ToUpper()
        );

        res.ShouldBe("HELLO WORLD");
    }

    [Fact]
    public void ShouldSelectMany()
    {
        var res =
            from greeting in Ok("Hello")
            from name in Ok("Andy")
            select string.Join(' ', greeting, name);

        res.ShouldBe("Hello Andy");
    }

    [Fact]
    public async Task ShouldSelectManyWhenFirstIsTask()
    {
        var res = await (
            from greeting in Task.FromResult(Ok("Hello"))
            from name in Ok("Andy")
            select string.Join(' ', greeting, name)
        );

        res.ShouldBe("Hello Andy");
    }

    [Fact]
    public async Task ShouldSelectManyWhenSecondIsTask()
    {
        var res = await (
            from greeting in Ok("Hello")
            from name in Task.FromResult(Ok("Andy"))
            select string.Join(' ', greeting, name)
        );

        res.ShouldBe("Hello Andy");
    }

    [Fact]
    public async Task ShouldSelectManyWhenBothAreTask()
    {
        var res = await (
            from greeting in Task.FromResult(Ok("Hello"))
            from name in Task.FromResult(Ok("Andy"))
            select string.Join(' ', greeting, name)
        );

        res.ShouldBe("Hello Andy");
    }

    [Fact]
    public void ShouldFilterWithSuccess()
    {
        var res =
            from name in Ok("Andy")
            where Ok(name + " is great")
            select name;

        res.ShouldBe("Andy");
    }

    [Fact]
    public void ShouldFilterWithError()
    {
        var res =
            from name in Ok("Donald")
            where Fail<int>(new Error(name + " sucks"))
            select name;

        res.ShouldBe(new Error("Donald sucks"));
    }

    [Fact]
    public async Task ShouldFilterFirstAsyncWithSuccess()
    {
        var res = await (
            from name in Task.FromResult(Ok("Andy"))
            where Ok(name + " is great")
            select name
        );

        res.ShouldBe("Andy");
    }

    [Fact]
    public async Task ShouldFilterFirstAsyncWithError()
    {
        var res = await (
            from name in Task.FromResult(Ok("Donald"))
            where Fail<int>(new Error(name + " sucks"))
            select name
        );

        res.ShouldBe(new Error("Donald sucks"));
    }

    [Fact]
    public async Task ShouldFilterSecondAsyncWithSuccess()
    {
        var res = await (
            from name in Ok("Andy")
            where Task.FromResult(Ok(name + " is great"))
            select name
        );

        res.ShouldBe("Andy");
    }

    [Fact]
    public async Task ShouldFilterSecondAsyncWithError()
    {
        var res = await (
            from name in Ok("Donald")
            where Task.FromResult(Fail<int>(new Error(name + " sucks")))
            select name
        );

        res.ShouldBe(new Error("Donald sucks"));
    }

    [Fact]
    public async Task ShouldFilterBothAsyncWithSuccess()
    {
        var res = await (
            from name in Task.FromResult(Ok("Andy"))
            where Task.FromResult(Ok(name + " is great"))
            select name
        );

        res.ShouldBe("Andy");
    }

    [Fact]
    public async Task ShouldFilterBothAsyncWithError()
    {
        var res = await (
            from name in Task.FromResult(Ok("Donald"))
            where Task.FromResult(Fail<int>(new Error(name + " sucks")))
            select name
        );

        res.ShouldBe(new Error("Donald sucks"));
    }
}