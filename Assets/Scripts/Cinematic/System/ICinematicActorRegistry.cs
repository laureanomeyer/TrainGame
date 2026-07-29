using UnityEngine;

public interface ICinematicActorRegistry
{
    void RegisterDynamic(string key, Transform actor);
    void UnregisterDynamic(string key);
    bool TryResolveDynamic(string key, out Transform actor);
}