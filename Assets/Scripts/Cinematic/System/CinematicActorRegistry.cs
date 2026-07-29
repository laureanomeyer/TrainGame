using System.Collections.Generic;
using UnityEngine;

public class CinematicActorRegistry : ICinematicActorRegistry
{
    private readonly Dictionary<string, Transform> _dynamicActors = new();

    public void RegisterDynamic(string key, Transform actor) => _dynamicActors[key] = actor;
    public void UnregisterDynamic(string key) => _dynamicActors.Remove(key);
    public bool TryResolveDynamic(string key, out Transform actor) => _dynamicActors.TryGetValue(key, out actor);


}
