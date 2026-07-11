using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static Dictionary<Type, object> services = new();

    public static void Register<T>(T service)
    {
        Type type = typeof(T);

        if (services.ContainsKey(type))
        {
            services[type] = service;
            return;
        }

        services.Add(type, service);
        Debug.Log("Servicio de tipo!" + type.ToString() + " registrado");
    }

    public static T Get<T>()
    {
        Type type = typeof(T);

        if (services.TryGetValue(type, out object service))
        {
            Debug.Log("Entregue servicio del tipo: " + type.ToString());
            return (T)service;
        }

        throw new Exception($"Service of type {type} not registered.");
    }

    public static bool TryGet<T>(out T service)
    {
        Type type = typeof(T);

        if (services.TryGetValue(type, out object foundService))
        {
            service = (T)foundService;
            return true;
        }

        service = default;
        return false;
    }

    public static void Unregister<T>()
    {
        Type type = typeof(T);

        if (services.ContainsKey(type))
        {
            services.Remove(type);
        }
    }

    public static void Clear()
    {
        Debug.Log("Clear");
        services.Clear();
    }
}