Using DI Container
This DI (Dependency Injection) container makes it easy to manage dependencies in your application and simplifies the process of Inversion of Control.

Binding
Before you can use services, you need to bind them to the DI container. To do this, use the Bind or BindSingle methods.
```
AllServices.BindSingle<IInputService>(new InputService());
AllServices.Bind<IPersistentProgressService, PersistentProgressService>();
AllServices.Bind<ISaveLoadService, SaveLoadService>();
AllServices.Bind<IGameFactory, GameFactory>();
AllServices.Bind<AssetProvider>();
```
Resolving Dependencies
After binding all the necessary services, call the ForceResolve() method to resolve all unresolved dependencies.
```
AllServices.ForceResolve();
```

Using Services
Now you can retrieve any service by calling the Single<TService>() method.
```
ISaveLoadService saveLoadService = AllServices.Single<ISaveLoadService>();
```

Full example
```
   private void RegisterServices()
    {
      AllServices.BindSingle<IInputService>(new InputService());
      AllServices.Bind<IPersistentProgressService, PersistentProgressService>();
      AllServices.Bind<ISaveLoadService, SaveLoadService>();
      AllServices.Bind<IGameFactory, GameFactory>();
      AllServices.Bind<AssetProvider>();
      
      AllServices.ForceResolve();
      
      AllServices.Single<ISaveLoadService>().SaveProgress();
    }
```
