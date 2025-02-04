using Microsoft.Extensions.DependencyInjection;

namespace Fyrstelin.Toolbox.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection KeyedDecorate<TService, TDecorator>(this IServiceCollection services, object key, object? fromKey = null)
        where TService : notnull
        where TDecorator : TService
    {
        return InnerDecorate<TService, TDecorator>(services, key, fromKey);
    }

    public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services, object? fromKey = null)
        where TService : notnull
        where TDecorator : TService
    {
        return InnerDecorate<TService, TDecorator>(services, null, fromKey);
    }

    private static IServiceCollection InnerDecorate<TService, TDecorator>(IServiceCollection services, object? key, object? fromKey)
        where TService : notnull
        where TDecorator : TService
    {
        var decoratee = services.LastOrDefault(x => x.ServiceType == typeof(TService) && x.ServiceKey == fromKey);
        if (decoratee == null) throw new InvalidOperationException($"No decoratee found for {typeof(TService).Name}.");

        var ctor = typeof(TDecorator).GetConstructors()
            .Single()
            .GetParameters()
            .Select(x => x.ParameterType)
            .ToList();

        if (!ctor.Contains(typeof(TService)))
        {
            throw new InvalidOperationException(
                $"{typeof(TDecorator).Name} is not a {typeof(TService).Name} decorator. Expected a constructor with a {typeof(TService).Name} parameter.");
        }

        var decorateeKey = fromKey == key ? new object() : fromKey;
        ChangeKey(services, [decoratee], decorateeKey);

        services.Add(new ServiceDescriptor(typeof(TService),
            key,
            (provider, _) => Activator.CreateInstance(
                typeof(TDecorator),
                ctor.Select(t => t == typeof(TService)
                    ? provider.GetRequiredKeyedService<TService>(decorateeKey)
                    : provider.GetRequiredService(t)).ToArray()
            )!,
            decoratee.Lifetime
        ));

            return services;
    }
}