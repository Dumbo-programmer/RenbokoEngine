using System;
using System.Collections.Generic;

namespace RenbokoEngine.Core
{
    // Marker interface for services registered in the ServiceLocator
    public interface IService { }

    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        // Register a service instance (T must implement IService)
        public static void Register<T>(T instance) where T : class, IService
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _services[typeof(T)] = instance;
        }

        // Get a service; throws if missing (existing behavior)
        public static T Get<T>() where T : class, IService
        {
            if (!_services.TryGetValue(typeof(T), out var inst))
                throw new KeyNotFoundException($"Service not registered: {typeof(T).FullName}");
            return (T)inst;
        }

        // Try to get a service without throwing
        public static bool TryGet<T>(out T? instance) where T : class, IService
        {
            if (_services.TryGetValue(typeof(T), out var inst))
            {
                instance = inst as T;
                return instance != null;
            }
            instance = null;
            return false;
        }

        // Convenience check
        public static bool Has<T>() where T : class, IService => _services.ContainsKey(typeof(T));

        // Optional: remove/unregister a service
        public static bool Unregister<T>() where T : class, IService => _services.Remove(typeof(T));
    }
}
