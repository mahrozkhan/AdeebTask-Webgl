using System;
using System.Collections.Generic;

namespace AdeebTask.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>(16);

        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var svc))
                return (T)svc;
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var svc))
            {
                service = (T)svc;
                return true;
            }
            service = null;
            return false;
        }

        public static void Reset() => _services.Clear();
    }
}
