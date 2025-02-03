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

    private static void ChangeKey(IServiceCollection services, IEnumerable<ServiceDescriptor> descriptors, object? componentsKey)
    {
        var serviceDescriptors = descriptors.ToList();
        foreach (var descriptor in serviceDescriptors)
        {
            services.Remove(descriptor);
        }

        services.Add(serviceDescriptors.Select(x =>
        {
            if (x.IsKeyedService)
            {
                if (x.KeyedImplementationType is not null)
                {
                    return new ServiceDescriptor(x.ServiceType, componentsKey, x.KeyedImplementationType, x.Lifetime);
                }

                if (x.KeyedImplementationInstance is not null)
                {
                    return new ServiceDescriptor(x.ServiceType, componentsKey, x.KeyedImplementationInstance);
                }

                if (x.KeyedImplementationFactory is not null)
                {
                    return new ServiceDescriptor(x.ServiceType, componentsKey, x.KeyedImplementationFactory, x.Lifetime);
                }
            }
            else
            {
                if (x.ImplementationType is not null)
                {
                    return new ServiceDescriptor(x.ServiceType, componentsKey, x.ImplementationType, x.Lifetime);
                }

                if (x.ImplementationInstance is not null)
                {
                    return new ServiceDescriptor(x.ServiceType, componentsKey, x.ImplementationInstance);
                }


                if (x.ImplementationFactory is not null)
                {
                    return new ServiceDescriptor(x.ServiceType, componentsKey, (provider, _) => x.ImplementationFactory(provider), x.Lifetime);
                }
            }

            throw new NotImplementedException($"Cannot create new service descriptor for {x.Lifetime} {x.ServiceType}");
        }));
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
        throw new InvalidOperationException($"Cannot convert IEnumerable<{typeof(T).Name}> to {to.Name}?");
    }
}