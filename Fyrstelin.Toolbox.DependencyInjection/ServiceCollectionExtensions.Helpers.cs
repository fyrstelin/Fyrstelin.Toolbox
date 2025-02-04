using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fyrstelin.Toolbox.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
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

}