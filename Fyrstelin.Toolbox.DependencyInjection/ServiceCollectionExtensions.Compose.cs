﻿using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fyrstelin.Toolbox.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection Compose<TService, TComposite>(
        this IServiceCollection services,
        object? fromKey = null
    ) where TComposite : class, TService where TService : class =>
        InnerCompose<TService, TComposite>(services, null, fromKey);

    public static IServiceCollection KeyedCompose<TService, TComposite>(
        this IServiceCollection services,
        object key,
        object? fromKey = null
    ) where TComposite : class, TService where TService : class =>
        InnerCompose<TService, TComposite>(services, key, fromKey);

    private static IServiceCollection InnerCompose<TService, TComposite>(IServiceCollection services, object? key, object? fromKey)
        where TComposite : class, TService where TService : class
    {
        var constructors = typeof(TComposite).GetConstructors();

        if (constructors.Length != 1)
        {
            throw new InvalidOperationException($"{typeof(TComposite).Name} have {constructors.Length} public constructors. Exactly one is expected.");
        }

        var converters = constructors
            .Single()
            .GetParameters()
            .Select(x => (type: x.ParameterType, convert: BuildConverter<TService>(x.ParameterType)))
            .ToList();

        if (!converters.Any(p => p.type.IsAssignableTo(typeof(IEnumerable<TService>))))
        {
            throw new InvalidOperationException($"{typeof(TComposite).Name} is not a composite of {typeof(TService).Name}.");
        }


        var components = services
            .Where(x => x.ServiceType == typeof(TService) && x.ServiceKey == fromKey)
            .ToList();
        
        var lifetime = components
            .Select(x => x.Lifetime)
            .Append(ServiceLifetime.Singleton)
            .Max();
        

        var componentsKey = key == fromKey ? new object() : fromKey;
        
        ChangeKey(services, components, componentsKey);

        services.Add(new ServiceDescriptor(typeof(TService), key, (provider, _) =>
        {
            return Activator.CreateInstance(
                typeof(TComposite),
                converters.Select(p =>
                    p.convert is not null
                        ? p.convert(provider.GetKeyedServices<TService>(componentsKey))
                        : provider.GetRequiredService(p.type)
                ).ToArray()
            )!;
        }, lifetime));

        return services;
    }


    private static Func<IEnumerable<T>, object>? BuildConverter<T>(Type to)
    {
        if (!typeof(IEnumerable<T>).IsAssignableFrom(to)) return null;

        if (typeof(IEnumerable<T>).IsAssignableTo(to)) return e => e;

        if (typeof(T[]).IsAssignableTo(to)) return e => e.ToArray();

        if (typeof(ImmutableList<T>).IsAssignableTo(to)) return e => e.ToImmutableList();

        if (to.IsClass)
        {
            var ctor = to.GetConstructor([typeof(IEnumerable<T>)]);
            if (ctor is not null)
            {
                return e => Activator.CreateInstance(to, e)!;
            }
        }


        return null;
    }
}