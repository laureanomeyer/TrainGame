using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static Dictionary<Type, object> services = new();

    public static void Register<T>(T service) where T : class
    {
        services[typeof(T)] = service;
    }

    public static T Get<T>() where T : class
    {
        if (TryGet(out T service))
            return service;

        throw new Exception($"Service of type {typeof(T)} not registered (or was destroyed).");
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        Type type = typeof(T);

        if (services.TryGetValue(type, out object found))
        {
            // Detecta "fake null" de objetos de Unity destruidos
            if (found is UnityEngine.Object unityObj && unityObj == null)
            {
                services.Remove(type);
                service = null;
                return false;
            }

            service = (T)found;
            return true;
        }

        service = null;
        return false;
    }

    public static void Unregister<T>() where T : class
    {
        services.Remove(typeof(T));
    }

    public static void Unregister<T>(T service) where T : class
    {
        Type type = typeof(T);

        if (services.TryGetValue(type, out object current) && ReferenceEquals(current, service))
            services.Remove(type);
    }

    public static void Clear()
    {
        services.Clear();
    }
}