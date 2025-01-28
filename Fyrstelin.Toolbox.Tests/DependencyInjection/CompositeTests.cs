using System.Collections.Immutable;
using Fyrstelin.Toolbox.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Fyrstelin.Toolbox.Tests.DependencyInjection;

public class CompositeTests
{
    [Fact]
    public void ShouldCompose()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IService, ServiceA>()
            .AddScoped<IService, ServiceB>()
            .AddTransient<IService, ServiceC>()
            .AddKeyedTransient<IService, ServiceD>("key")
            .Compose<IService, Composite>();

        using var provider = services.BuildServiceProvider();

        var shouldBeOfType = provider.GetRequiredService<IService>()
            .ShouldBeOfType<Composite>();
        shouldBeOfType
            .Services.ShouldBe([new ServiceA(), new ServiceB(), new ServiceC()]);
    }

    [Fact]
    public void ShouldKeyedCompose()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IService, ServiceA>()
            .AddScoped<IService, ServiceB>()
            .AddTransient<IService, ServiceC>()
            .AddKeyedTransient<IService, ServiceD>("key")
            .KeyedCompose<IService, Composite>("another-key");

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredKeyedService<IService>("another-key")
            .ShouldBeOfType<Composite>()
            .Services.ShouldBe([new ServiceA(), new ServiceB(), new ServiceC()]);

        provider.GetServices<IService>().Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldComposeIntoSingleton()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IService, ServiceA>()
            .AddKeyedTransient<IService, ServiceD>("key")
            .Compose<IService, Composite>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        provider
            .GetRequiredService<IService>()
            .ShouldBe(scope.ServiceProvider.GetRequiredService<IService>());
    }

    [Fact]
    public void ShouldComposeIntoScoped()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IService, ServiceA>()
            .AddScoped<IService, ServiceB>()
            .AddKeyedTransient<IService, ServiceD>("key")
            .Compose<IService, Composite>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var outerService = provider
            .GetRequiredService<IService>();

        var innerService1 = scope.ServiceProvider.GetRequiredService<IService>();
        var innerService2 = scope.ServiceProvider.GetRequiredService<IService>();
        
        outerService.ShouldNotBe(innerService1);
        innerService1.ShouldBe(innerService2);
    }

    [Fact]
    public void ShouldComposeIntoTransient()
    {
        var services = new ServiceCollection();
     
        services
            .AddSingleton<IService, ServiceA>()
            .AddScoped<IService, ServiceB>()
            .AddTransient<IService, ServiceC>()
            .AddKeyedTransient<IService, ServiceD>("key")
            .Compose<IService, Composite>();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IService>()
            .ShouldNotBe(provider.GetRequiredService<IService>());
    }

    [Fact]
    public void ShouldNotRemoveKeysServices()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IService, ServiceA>()
            .AddScoped<IService, ServiceB>()
            .AddTransient<IService, ServiceC>()
            .AddKeyedTransient<IService, ServiceD>("key")
            .Compose<IService, Composite>();

        using var provider = services.BuildServiceProvider();

        provider.GetKeyedService<IService>("key")
            .ShouldNotBeNull()
            .ShouldBeOfType<ServiceD>();
    }

    [Fact]
    public void ShouldComposeFromKeyedServices()
    {

        var services = new ServiceCollection();

        services
            .AddKeyedSingleton<IService, ServiceA>("key")
            .AddKeyedScoped<IService, ServiceB>("key")
            .AddKeyedTransient<IService, ServiceC>("key")
            .AddTransient<IService, ServiceD>()
            .Compose<IService, Composite>("key");
        
        using var provider = services.BuildServiceProvider();

        var registeredServices = provider.GetServices<IService>().ToList();

        registeredServices.Count.ShouldBe(2);
        registeredServices
            .OfType<ServiceD>()
            .ShouldBe([new ServiceD()]);
        registeredServices
            .OfType<Composite>()
            .Single()
            .Services.ShouldBe([
                new ServiceA(),
                new ServiceB(),
                new ServiceC()
            ]);

        provider.GetKeyedServices<IService>("key").Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldKeyedComposeFromKeyedServices()
    {

        var services = new ServiceCollection();

        services
            .AddKeyedSingleton<IService, ServiceA>("key")
            .AddKeyedScoped<IService, ServiceB>("key")
            .AddKeyedTransient<IService, ServiceC>("key")
            .AddKeyedTransient<IService, ServiceD>("another-key")
            .KeyedCompose<IService, Composite>("another-key", "key");

        using var provider = services.BuildServiceProvider();
        
        var registeredServices = provider.GetKeyedServices<IService>("another-key").ToList();

        registeredServices.Count.ShouldBe(2);
        registeredServices
            .OfType<ServiceD>()
            .ShouldBe([new ServiceD()]);
        registeredServices
            .OfType<Composite>()
            .Single()
            .Services.ShouldBe([
                new ServiceA(),
                new ServiceB(),
                new ServiceC()
            ]);

        provider.GetKeyedServices<IService>("key").Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldRegisterAsSingletonWhenNoComponentsAreRegistered()
    {
        var services = new ServiceCollection();
        services.Compose<IService, Composite>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        provider.GetService<IService>().ShouldBe(scope.ServiceProvider.GetService<IService>());
    }

    [Fact]
    public void ShouldFailForBadComposites()
    {
        var services = new ServiceCollection();
        Should.Throw<InvalidOperationException>(() => services.Compose<IService, ServiceA>())
            .Message.ShouldBe("ServiceA is not a composite of IService.");
    }

    [Fact]
    public void ShouldFailForMissingConstructor()
    {
        var services = new ServiceCollection();

        Should.Throw<InvalidOperationException>(() => services.Compose<IService, MultipleConstructor>())
            .Message.ShouldBe("MultipleConstructor have 2 public constructors. Exactly one is expected.");
    }

    [Fact]
    public void ShouldSupportDifferentLists()
    {
        var services = new ServiceCollection();

        var fromKey = new object();
        services
            .Compose<IService, Composite>(fromKey)
            .Compose<IService, ReadonlyListComposite>(fromKey)
            .Compose<IService, ImmutableListComposite>(fromKey)
            .Compose<IService, ListComposite>(fromKey)
            .Compose<IService, SetComposite>(fromKey)
            .BuildServiceProvider()
            .GetServices<IService>()
        ;
    }

    public interface IService;

    public record ServiceA : IService;

    public record ServiceB : IService;
    public record ServiceC : IService;
    public record ServiceD : IService;

    public class MultipleConstructor : IService
    {
        public string Input { get; }

        public MultipleConstructor(string input)
        {
            Input = input;
        }

        public MultipleConstructor(int input)
        {
            Input = input.ToString();
        }
    }

    public record Composite(IEnumerable<IService> Services) : IService;
    public record ReadonlyListComposite(IReadOnlyList<IService> Services) : IService;
    public record ImmutableListComposite(IImmutableList<IService> Services) : IService;
    public record ListComposite(List<IService> Services) : IService;
    public record SetComposite(HashSet<IService> Services) : IService;
}