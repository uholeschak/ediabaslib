using System.Collections.Generic;
using System;

namespace PsdzClient.Core
{
    public sealed class ServiceLocator : IServiceLocator
    {
        private static readonly IServiceLocator instance = new ServiceLocator();
        private readonly Dictionary<Type, object> services;
        public static IServiceLocator Current => instance;

        private ServiceLocator()
        {
            services = new Dictionary<Type, object>();
        }

        public T GetService<T>()
            where T : class
        {
            Type typeFromHandle = typeof(T);
            if (!services.ContainsKey(typeFromHandle))
            {
                Log.Error("ServiceLocator.GetService<T>()", "No service registered for type \"{0}\". Using default ({1}) instead.", typeFromHandle.FullName, null);
                services.Add(typeFromHandle, null);
            }

            return (T)services[typeFromHandle];
        }

        public void AddService<T>(T service)
            where T : class
        {
            if (service == null)
            {
                throw new ArgumentException("Parameter \"service\" must not be null.");
            }

            Type typeFromHandle = typeof(T);
            if (services.ContainsKey(typeFromHandle))
            {
                throw new InvalidOperationException($"Will not register service {service}, because the service \"{services[typeFromHandle]}\" is already registered for the type \"{typeFromHandle}\".");
            }

            services.Add(typeFromHandle, service);
        }

        public void TryAddService<T>(T service)
            where T : class
        {
            if (service == null)
            {
                throw new ArgumentException("Parameter \"service\" must not be null.");
            }

            Type typeFromHandle = typeof(T);
            if (GetService<T>() == null)
            {
                RemoveService<T>();
            }

            if (!services.ContainsKey(typeFromHandle))
            {
                services.Add(typeFromHandle, service);
            }
        }

        public void RemoveService<T>()
            where T : class
        {
            Type typeFromHandle = typeof(T);
            if (services.ContainsKey(typeFromHandle))
            {
                services.Remove(typeFromHandle);
            }
        }

        public bool TryGetService<T>(out T service)
            where T : class
        {
            Type typeFromHandle = typeof(T);
            if (!services.ContainsKey(typeFromHandle))
            {
                service = null;
                return false;
            }

            service = (T)services[typeFromHandle];
            return true;
        }
    }
}