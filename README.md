# Service Locator
A service locator implementation that is both lightweight and powerful.

It can be used for **replacing static classes** with an interface-implementation pair.
This can be used to
- easily replace implementations at compile time or during startup.
- write unit tests for service user classes where the actual implementation is replaced with a mock/substitute instance.

It is **not** meant for anything beyond that.

## Usage
**Import** via
```csharp
using TeaSpoons.ServiceLocator;
```

### Getting service references
- Use `Services.Get` to **get the reference** to a service.
```csharp
var musicService = Services.Get<IMusicService>()
musicService.Play(...);
```

### Binding Services
Services are supposed to act like static classes, so
- They must be bound early.<br>Use the `[RuntimeInitializeOnLoadMethod]` attribute with the `ServiceBinder.Phase` parameter to bind your service.
- They cannot be unbound and stay valid for the entirety of the runtime.

Use the following pattern to bind a service:
```csharp
public interface IMusicService : IService
{
  void Play(...);
}

internal class MusicService : IMusicService
{
  #region Service Initialization
  [RuntimeInitializeOnLoadMethod(ServiceBinder.Phase)]
  private static void BindService()
  {
    ServiceBinder.For<IMusicService>().Bind(() => new MusicService());
  }
  #endregion

  // ...
}
```
Note: All interfaces that can be bound to must inherit the empty interface `IService`, solely to mark them as services that will be used like static classes.

Note: The `Func<T>` that is passed to the `Bind` method creates the instance that will be bound to the interface. The function will *only* be called *if* the binding is *actually applied* (rather than being prevented by an _expectation_, see below).
Thus, doing heavy initialization work (including, for example, instantiating prefabs) is fine in the function or the constructor of the service class.
```csharp
  private MusicService()
  {
    var musicPlayerPrefab = LoadMusicPlayerPrefab();
    Instantiate(musicPlayerPrefab);
  }
```

However, you can also wait until the `AfterSceneLoaded` phase of Unity's initialization by subscribing to the `InitialSceneLoaded` event.
```csharp
private MusicService()
{
  ServiceBinder.AfterSceneLoaded += () =>
  {
    var musicPlayerPrefab = LoadMusicPlayerPrefab();
    Instantiate(musicPlayerPrefab);
  }
}
```

### Cross-service interaction during startup
Sometimes, a service is dependent on another service as early as during startup.

In that case, use the `BindingFinished` callback in a service constructor to ensure that all services are already bound:
```csharp
private MusicService()
{
  ServiceBinder.BindingFinished += () =>
  {
    var anotherService = Services.Get<IAnotherService>();
    anotherService.DoSomething();
  };
}
``` 

### Binding Expectations
A setup script that binds "from the outside" (like in dependency injection) also works.
But it's instead recommended to have services bind themselves instead.

If you have a situation in which multiple implementations exist and would cause a collision,
it is recommended to have a central setup script that creates "expectations".
This setup script would also use the `ServiceBinder.Phase`.

It would look like this:
```csharp
[RuntimeInitializeOnLoadMethod(ServiceBinder.Phase)]
private static void SetupServiceExpectations()
{
  ServiceExpectations.TryAdd<IMusicPlayer, LoudMusicPlayer>();
}
```

Expectations work as follows:
- When `Bind` is called for an interface `I` and an object of type `T`,
  - and an expectation for `I` is set,
    - and the expectation is set for `T`,
      - ...the call will *succeed*.
    - and the expectation is set for a different type than `T`,
      - ...the call will *silently fail*.
  - and **no** expectation for `I` is set,
    - ...the call will succeed if no binding for `I` currently exists. The second call will cause a `DuplicateBindingException`.

### Using Mocks/Substitute Services in Unit Testing
In unit testing, having mocks/substitutes means that you can test a code unit without testing its depdendencies.

The service locator can be used to supply a to-be-tested object with mock/substitute dependencies.
The class to use for this is called `ServicesTestMode`. It is **only available in the editor**.

Use `ServicesTestMode.Start()` in your unit test `SetUp` method to enable **test mode** until the end of the session.

With test mode enabled, you can bind services at any time, even after initial binding has happened.

Calling `Start` also deletes all existing bindings and expectations, allowing you to build your service bindings from scratch.

## Installation

In Unity: **Window > Package Manager > + > Add package from git URL**, then enter:

```
https://github.com/tea-spoons/service-locator.git
```

Pin a release by appending a tag, for example `#v0.5.7`.

## Change plan

See [CHANGE-PLAN.md](CHANGE-PLAN.md) for what changed before publishing and what is planned next.

## License

Copyright (c) 2026 Bigpoint. Authored by Muhammad Tarek Abdou.

Available for research, education and other noncommercial use under the [PolyForm Noncommercial 1.0.0](LICENSE.md)
license. Commercial use is not permitted.
