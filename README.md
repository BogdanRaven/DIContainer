Using DI Container
This DI (Dependency Injection) container makes it easy to manage dependencies in your Unity project and simplifies the process of Inversion of Control.

## 🔧 Installation
You can install this package in Unity using **Unity Package Manager**:
1. Open `Window` → `Package Manager`.
2. Click `+` → `Add package from git URL...`.
3. Enter: https://github.com/BogdanRaven/DIContainer.git
4. Click `Add`, and Unity will download the package.

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
