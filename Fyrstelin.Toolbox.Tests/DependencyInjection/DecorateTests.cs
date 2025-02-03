using Fyrstelin.Toolbox.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Fyrstelin.Toolbox.Tests.DependencyInjection;

public class DecorateTests
{
    [Fact]
    public void ShouldDecorate()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IService, ServiceA>();
        services.Decorate<IService, Decorator>();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IService>()
            .ShouldBeOfType<Decorator>()
            .Decoratee.ShouldBeOfType<ServiceA>();
    }

    [Fact]
    public void ShouldDecorateTwice()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IService, ServiceA>();
        services.Decorate<IService, Decorator>();
        services.Decorate<IService, Decorator>();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IService>()
            .ShouldBeOfType<Decorator>()
            .Decoratee
            .ShouldBeOfType<Decorator>()
            .Decoratee
            .ShouldBeOfType<ServiceA>();
    }

    [Fact]
    public void ShouldDecorateSingletonService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IService, ServiceA>();
        services.Decorate<IService, Decorator>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        provider.GetRequiredService<IService>()
            .ShouldBe(scope.ServiceProvider.GetRequiredService<IService>());
    }

    [Fact]
    public void ShouldDecorateScopedService()
    {
        var services = new ServiceCollection();
        services.AddScoped<IService, ServiceA>();
        services.Decorate<IService, Decorator>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        provider.GetRequiredService<IService>()
            .ShouldNotBe(scope.ServiceProvider.GetRequiredService<IService>());
    }

    [Fact]
    public void ShouldDecorateTransientService()
    {
        var services = new ServiceCollection();
        services.AddTransient<IService, ServiceA>();
        services.Decorate<IService, Decorator>();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IService>()
            .ShouldNotBe(provider.GetRequiredService<IService>());
    }

    [Fact]
    public void ShouldDecorateKeyedService()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<IService, ServiceA>()
            .AddKeyedSingleton<IService, ServiceB>("some key")
            .Decorate<IService, Decorator>("some key")
        ;

        using var provider = services.BuildServiceProvider();
        provider
            .GetRequiredService<IService>()
            .ShouldBeOfType<Decorator>()
            .Decoratee
            .ShouldBeOfType<ServiceB>();

        provider.GetServices<IService>().Count().ShouldBe(2);
        provider.GetKeyedServices<IService>("some key").Count().ShouldBe(1);
    }

    [Fact]
    public void ShouldKeyedDecorate()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<IService, ServiceA>()
            .KeyedDecorate<IService, Decorator>("my-key");

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IService>()
            .ShouldBeOfType<ServiceA>();

        provider.GetRequiredKeyedService<IService>("my-key")
            .ShouldBeOfType<Decorator>();
    }

    [Fact]
    public void ShouldKeyedDecorateKeyedService()
    {
        var services = new ServiceCollection();
        services
            .AddKeyedSingleton<IService, ServiceA>("my-key")
            .AddKeyedSingleton<IService, ServiceA>("key")
            .KeyedDecorate<IService, Decorator>("my-key", "key");

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredKeyedService<IService>("key")
            .ShouldBeOfType<ServiceA>();

        provider.GetRequiredKeyedService<IService>("my-key")
            .ShouldBeOfType<Decorator>();

        provider.GetKeyedServices<IService>("my-key").Count().ShouldBe(2);
        provider.GetKeyedServices<IService>("key").Count().ShouldBe(1);
    }

    [Fact]
    public void ShouldFailWhenNothingToDecorate()
    {
        var services = new ServiceCollection();

        Should.Throw<InvalidOperationException>(() => services.Decorate<IService, Decorator>())
            .Message.ShouldBe("No decoratee found for IService.");
    }

    [Fact]
    public void ShouldFailWhenDecoratorIsInvalid()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IService, ServiceA>();

        Should.Throw<InvalidOperationException>(() => services.Decorate<IService, ServiceA>())
            .Message.ShouldBe("ServiceA is not a IService decorator. Expected a constructor with a IService parameter.");

    }

    public interface IService;

    public class ServiceA : IService;
    public class ServiceB : IService;

    public class Decorator(IService decoratee) : IService
    {
        public IService Decoratee { get; } = decoratee;
    }
}