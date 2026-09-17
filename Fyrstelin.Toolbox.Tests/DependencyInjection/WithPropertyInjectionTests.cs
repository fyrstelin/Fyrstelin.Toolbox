using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Fyrstelin.Toolbox.Tests.DependencyInjection;

public class WithPropertyInjectionTests
{
    [Fact]
    public void ShouldInjectRequiredProperties()
    {
        var services = new ServiceCollection()
            .WithPropertyInjection()
            .AddSingleton<IService, MyService>()
            .AddSingleton("Dependency")
            .AddKeyedSingleton("KeyedDependency", "Something Else");

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IService>().ShouldBeOfType<MyService>();
        
        service.Dependency.ShouldBe("Dependency");
        service.KeyedDependency.ShouldBe("Something Else");
    }

    [Fact]
    public void ShouldInjectRequiredPropertiesIntoKeyedService()
    {
        var services = new ServiceCollection()
            .WithPropertyInjection()
            .AddKeyedSingleton<IService, MyService>("MyService")
            .AddSingleton("Dependency")
            .AddKeyedSingleton("KeyedDependency", "Something Else");

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredKeyedService<IService>("MyService").ShouldBeOfType<MyService>();
        
        service.Dependency.ShouldBe("Dependency");
        service.KeyedDependency.ShouldBe("Something Else");
    }

    

    public interface IService;

    public class MyService : IService
    {
        public required string Dependency { get; init; }
        
        [Key("KeyedDependency")]
        public required string KeyedDependency { get; init; }
    }
}
