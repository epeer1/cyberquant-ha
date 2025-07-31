using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace FSScore.WebApi
{
    /// <summary>
    /// Simple dependency injection container for .NET Framework 4.8 Web API
    /// </summary>
    public class SimpleDependencyContainer : IDependencyResolver
    {
        private readonly Dictionary<Type, Func<IDependencyResolver, object>> _services;
        private readonly Dictionary<Type, object> _singletons;

        public SimpleDependencyContainer()
        {
            _services = new Dictionary<Type, Func<IDependencyResolver, object>>();
            _singletons = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Register a transient service (new instance each time)
        /// </summary>
        public void RegisterTransient<T>(Func<IDependencyResolver, T> factory)
        {
            _services[typeof(T)] = resolver => factory(resolver);
        }

        /// <summary>
        /// Register a singleton service (same instance each time)
        /// </summary>
        public void RegisterSingleton<T>(Func<T> factory)
        {
            _services[typeof(T)] = resolver =>
            {
                if (!_singletons.ContainsKey(typeof(T)))
                {
                    _singletons[typeof(T)] = factory();
                }
                return _singletons[typeof(T)];
            };
        }

        /// <summary>
        /// Get a service instance
        /// </summary>
        public object GetService(Type serviceType)
        {
            if (_services.ContainsKey(serviceType))
            {
                return _services[serviceType](this);
            }
            return null;
        }

        /// <summary>
        /// Get a service instance (generic)
        /// </summary>
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// Get all services of a type (not implemented for simplicity)
        /// </summary>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var service = GetService(serviceType);
            if (service != null)
            {
                yield return service;
            }
        }

        /// <summary>
        /// Begin dependency scope (returns self for simplicity)
        /// </summary>
        public IDependencyScope BeginScope()
        {
            return this;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            foreach (var singleton in _singletons.Values)
            {
                if (singleton is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _singletons.Clear();
            _services.Clear();
        }
    }
}