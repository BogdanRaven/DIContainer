using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.Services.DIContainer
{
    public class AllServices
    {
        private readonly Dictionary<Type, IService> _resolvedServices = new();
        private readonly Dictionary<Type, IService> _unresolvedBindings = new();

        private readonly Dictionary<Type, IService> _allServices = new();

        private static AllServices _instance;
        private ServicesResolver _servicesResolver;

        private AllServices() =>
            _servicesResolver = new ServicesResolver(_resolvedServices, _unresolvedBindings, _allServices);

        public static AllServices Container
        {
            get
            {
                if (_instance != null)
                    return _instance;

                return _instance = new AllServices();
            }
        }

        public void Bind<TService>() where TService : IService => Bind<TService, TService>();

        public void Bind<TService, TImplementation>() where TService : IService where TImplementation : TService
        {
            ValidateImplementationType<TImplementation>();

            var service = Resolve<TService, TImplementation>();

            if (service.Item2 == false)
                _unresolvedBindings[typeof(TService)] = service.Item1;
            else
                _resolvedServices[typeof(TService)] = service.Item1;

            _allServices[typeof(TService)] = service.Item1;
        }

        public void Dispose<TService>() where TService : IDisposableService =>
            ((IDisposableService)_resolvedServices[typeof(TService)])?.Dispose();

        public void BindSingle<TService>(TService implementation) where TService : IService
        {
            _resolvedServices[typeof(TService)] = implementation;
            _allServices[typeof(TService)] = implementation;
        }

        public TService Single<TService>() where TService : class, IService =>
            GetService<TService>();

        public void ForceResolve() =>
            _servicesResolver.ForceResolve();

        private TService GetService<TService>() where TService : class, IService
        {
            if (_resolvedServices.ContainsKey(typeof(TService)))
                return _resolvedServices[typeof(TService)] as TService;

            throw new Exception($"Service of type {typeof(TService).Name} is not registered.");
        }

        private (IService, bool) Resolve<TService, TImplementation>()
            where TService : IService where TImplementation : TService =>
            _servicesResolver.Resolve<TService, TImplementation>();

        private void ValidateImplementationType<TImplementation>()
        {
            Type type = typeof(TImplementation);

            if (type.IsAbstract)
                throw new InvalidOperationException($"Type {type.Name} is abstract.");

            if (type.IsInterface)
                throw new InvalidOperationException($"Type {type.Name} is interface.");
        }
    }
}