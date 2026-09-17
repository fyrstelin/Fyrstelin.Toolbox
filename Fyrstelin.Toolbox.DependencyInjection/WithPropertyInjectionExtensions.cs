using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute(object key) : Attribute
{
    public object Key { get; } = key;
}

public static class WithPropertyInjectionExtensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection WithPropertyInjection() => new Provider(collection);
    }

    private class Provider(IServiceCollection decoratee) : IServiceCollection
    {
        private readonly object _key = new();

        public ServiceDescriptor this[int index] => decoratee[index];

        ServiceDescriptor IList<ServiceDescriptor>.this[int index] { 
            get => decoratee[index];
            set => decoratee[index] = value;
        }

        public int Count => decoratee.Count;

        public bool IsReadOnly => decoratee.IsReadOnly;

        public void Add(ServiceDescriptor item)
        {
            if (!item.IsKeyedService && item.ImplementationType is not null)
            {
                var requiredProperties = item.ImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && p.GetCustomAttribute<RequiredMemberAttribute>() is not null);

                decoratee.Add(ServiceDescriptor.DescribeKeyed(item.ServiceType, _key, item.ImplementationType, item.Lifetime));
                decoratee.Add(ServiceDescriptor.Describe(item.ServiceType, provider =>
                {
                    var service = provider.GetRequiredKeyedService(item.ServiceType, _key);
                    foreach (var property in requiredProperties)
                    {
                        if (property.GetCustomAttribute<KeyAttribute>() is KeyAttribute keyAttribute)
                            property.SetValue(service, provider.GetRequiredKeyedService(property.PropertyType, keyAttribute.Key));
                        else
                            property.SetValue(service, provider.GetRequiredService(property.PropertyType));
                    }
                    return service;
                }, item.Lifetime));                
            }
            else if (item.IsKeyedService && item.KeyedImplementationType is not null)
            {
                var requiredProperties = item.KeyedImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && p.GetCustomAttribute<RequiredMemberAttribute>() is not null);

                decoratee.Add(ServiceDescriptor.DescribeKeyed(item.ServiceType, (_key, item.ServiceKey), item.KeyedImplementationType, item.Lifetime));
                decoratee.Add(ServiceDescriptor.DescribeKeyed(item.ServiceType, item.ServiceKey, (provider, key) =>
                {
                    var service = provider.GetRequiredKeyedService(item.ServiceType, (_key, item.ServiceKey));
                    foreach (var property in requiredProperties)
                    {
                        if (property.GetCustomAttribute<KeyAttribute>() is KeyAttribute keyAttribute)
                            property.SetValue(service, provider.GetRequiredKeyedService(property.PropertyType, keyAttribute.Key));
                        else
                            property.SetValue(service, provider.GetRequiredService(property.PropertyType));
                    }
                    return service;
                }, item.Lifetime));   
            }
            else
            {
                decoratee.Add(item);
            }

        }

        public void Clear() => decoratee.Clear();

        public bool Contains(ServiceDescriptor item) => decoratee.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => decoratee.CopyTo(array, arrayIndex);

        public IEnumerator<ServiceDescriptor> GetEnumerator() => decoratee.GetEnumerator();

        public int IndexOf(ServiceDescriptor item) => decoratee.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item) => decoratee.Insert(index, item);

        public bool Remove(ServiceDescriptor item) => decoratee.Remove(item);

        public void RemoveAt(int index) => decoratee.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
