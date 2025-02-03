# Fyrstelin.Toolbox.DependencyInjection
Structural design patters for dependency inject.

### Services
```c#
public interface IService;
public class ServiceA : IService;
public class ServiceB : IService;
public class Decorator(IService decoratee) : IService;
public class Composite(IEnumberable<IService> children) : IService;
```

## Decorator Pattern

### Simple usage
```c#
services
  .AddSingleton<IService, ServiceA>()
  .Decorate<IService, Decorator>();

var service = provider.GetRequiredService<IService>();
//  ^ Decorator with ServiceA as decoratee
```

### Decorate a keyed service
```c#
services
  .AddKeyedSingleton<IService, ServiceA>("some key")
  .Decorate<IService, Decorator>("some key");

var decorator = provider.GetRequiredService<IService>();
//  ^ Decorator with ServiceA as decoratee
var service = provider.GetRequiredKeyedService<IService>("some key");
//  ^ ServiceA
```

### Keyed decorator
```c#
services
  .AddSingleton<IService, ServiceA>()
  .KeyedDecorate<IService, Decorator>("some key");

var decorator = provider.GetRequiredKeyedService<IService>("some key");
//  ^ Decorator with ServiceA as decoratee
var service = provider.GetRequired<IService>();
//  ^ ServiceA
```

## Composite Pattern

### Simple usage

```c#
services
  .AddSingleton<IService, ServiceA>()
  .AddScoped<IService, ServiceB>()
  .Compose<IService, Composite>();

var service = provider.GetRequiredService<IService>();
//  ^ Composite with ServiceA and ServiceB as children
```

### Compose keyed services

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

### Keyed composite

```c#
services
  .AddSingleton<IService, ServiceA>()
  .AddScoped<IService, ServiceB>()
  .KeyedCompose<IService, Composite>("my-key");

var service = provider.GetRequiredKeyedService<IService>("my-key");
//  ^ Composite with ServiceA and ServiceB as children

var services = provider.GetServices<IService>()
//  ^ IEnumerable<IService> containing ServiceA and ServiceB
```