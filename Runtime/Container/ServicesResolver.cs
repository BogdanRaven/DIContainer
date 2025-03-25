using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeBase.Infrastructure.Services.DIContainer
{
  public class ServicesResolver
  {
    private readonly Dictionary<Type, IService> _resolvedServices;
    private readonly Dictionary<Type, IService> _unresolvedBindings;
    private readonly Dictionary<Type, IService> _allServices;

    public ServicesResolver(Dictionary<Type, IService> resolvedServices, Dictionary<Type, IService> unresolvedBindings,
      Dictionary<Type, IService> allServices)
    {
      _resolvedServices = resolvedServices;
      _unresolvedBindings = unresolvedBindings;
      _allServices = allServices;
    }

    public (IService, bool) Resolve<TService, TImplementation>()
      where TService : IService where TImplementation : TService
    {
      ConstructorInfo constructor = GetConstructorInfo(typeof(TImplementation));

      if (constructor == null)
        return CreateInstance<TService, TImplementation>();

      List<IService> resolvedParameters = ResolveMethodParameters(constructor, out var resolvedStatus);

      (TService, bool resolvedStatus) service = (
        (TService)Activator.CreateInstance(typeof(TImplementation), resolvedParameters.ToArray()),
        resolvedStatus);

      return service;
    }

    public void ForceResolve()
    {
      if (HasCircleDependency(_allServices))
        throw new InvalidOperationException("Circular dependency detected in unresolved services.");

      var resolvedServices = new Dictionary<Type, IService>();

      while (_unresolvedBindings.Count != 0)
      {
        foreach (var (key, value) in _unresolvedBindings)
        {
          IService service = ResolveUnresolvedService(value);
          if (service != null)
            resolvedServices.Add(key, service);
        }

        foreach (var service in resolvedServices)
        {
          if (_resolvedServices.ContainsKey(service.Key))
            continue;

          _resolvedServices[service.Key] = service.Value;
          _unresolvedBindings.Remove(service.Key);
        }
      }
    }

    public IService ResolveUnresolvedService(IService service)
    {
      ConstructorInfo constructor = GetConstructorInfo(service.GetType());

      var resolvedParameters = ResolveMethodParameters(constructor, out var resolvedStatus);

      if (!resolvedStatus) return null;

      var newService = (Activator.CreateInstance(service.GetType(), resolvedParameters.ToArray()));

      return (IService)newService;
    }

    private List<IService> ResolveMethodParameters(ConstructorInfo constructor, out bool resolvedStatus)
    {
      ParameterInfo[] parameters = constructor.GetParameters();

      List<IService> resolvedParameters = ResolveMethodParameters(parameters, out resolvedStatus);
      return resolvedParameters;
    }

    private List<IService> ResolveMethodParameters(ParameterInfo[] parameters, out bool resolvedStatus)
    {
      List<IService> resolvedParameters = new List<IService>();
      resolvedStatus = true;

      foreach (var parameter in parameters)
      {
        Type resolvedParameter = parameter.ParameterType;

        if (_resolvedServices.TryGetValue(resolvedParameter, out var service))
          resolvedParameters.Add(service);
        else
        {
          resolvedParameters.Add(null);
          resolvedStatus = false;
        }
      }

      return resolvedParameters;
    }

    private ConstructorInfo GetConstructorInfo(Type implementation) =>
      implementation
        .GetConstructors()
        .FirstOrDefault(constructor => constructor.GetParameters().Any());

    private (TService, bool) CreateInstance<TService, TImplementation>()
      where TService : IService where TImplementation : TService =>
      ((TService)Activator.CreateInstance(typeof(TImplementation)), true);
    
    private bool HasCircleDependency(Dictionary<Type, IService> services)
    {
      HashSet<Type> visited = new HashSet<Type>();
      HashSet<Type> currentlyVisited = new HashSet<Type>();

      foreach (var serviceType in services.Keys)
      {
        if (HasCircleDependencyDFS(serviceType, services, visited, currentlyVisited))
          return true;
      }

      return false;
    }

    private bool HasCircleDependencyDFS(Type serviceType, Dictionary<Type, IService> services,
      HashSet<Type> visited, HashSet<Type> currentlyVisited)
    {
      if (currentlyVisited.Contains(serviceType))
        return true;

      if (visited.Contains(serviceType))
        return false;

      currentlyVisited.Add(serviceType);

      IService service;
      if (services.TryGetValue(serviceType, out var service1)) service = service1;
      else
        throw new Exception($"Type {serviceType.FullName} is not registered");

      ParameterInfo[] constructorParameters = service.GetType().GetConstructors().First().GetParameters();

      foreach (var parameter in constructorParameters)
      {
        if (HasCircleDependencyDFS(parameter.ParameterType, services, visited, currentlyVisited))
          return true;
      }

      currentlyVisited.Remove(serviceType);
      visited.Add(serviceType);

      return false;
    }
  }
}