# Fyrstelin.Toolbox.DependencyInjection
Structural design patters for dependency injcet.

## Composite Pattern
Sometimes a composite is needed. Dotnet does not support this out of the box due to circular dependencies. It can be solved with keyed services, but in my opinion your classes should not depend on the IoC framework. 

```c#
// services
public interface IService;
public class ServiceA : IService;
public class ServiceB : IService;
public class Composite(IEnumerable<IService> children) : IService;

// add services
services
  .AddSingleton<IService, ServiceA>()
  .AddScoped<IService, ServiceB>()
  .Compose<IService, Composite>();

// Get service (probably through dependency injection)
var service = provider.GetRequiredService<IService>();
//  ^ Composite with ServiceA and ServiceB as children
```

### Keyed services
You can compose your composite from keyed services:

```c#
services
  .AddKeyedSingleton<IService, ServiceA>("my-key")
  .AddKeyedScoped<IService, ServiceB>("my-key")
  .Compose<IService, Composite>("my-key");

var service = provider.GetRequiredService<IService>();
//  ^ Composite with ServiceA and ServiceB as children

var services = provider.GetKeyedServices<IService>("my-key")
//  ^ IEnumerable<IService> containing ServiceA and ServiceB

```

### Keyed compose
Just like the build in methids, a KeyedCompose exists:

```c#
services
  .KeyedCompose<IService, Composite>("my-key")
  .KeyedCompose<IService, Composite>("my-key", "from-key");
```