using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGameEvent { }
public static class EventBus
{
    public static void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        => EventBus<T>.Subscribe(handler);

    public static void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        => EventBus<T>.Unsubscribe(handler);

    public static void Publish<T>(in T ev) where T : struct, IGameEvent
        => EventBus<T>.Publish(in ev);

    // Para limpiar entre sesiones de Play (si tienes Domain Reload desactivado)
    internal static readonly List<Action> clearActions = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetAll()
    {
        foreach (var clear in clearActions) clear();
    }
}

internal static class EventBus<T> where T : struct, IGameEvent
{
    private static readonly List<Action<T>> handlers = new();
    private static int publishDepth;
    private static bool hasNulls;

    static EventBus()
    {
        EventBus.clearActions.Add(() =>
        {
            handlers.Clear();
            publishDepth = 0;
            hasNulls = false;
        });
    }

    public static void Subscribe(Action<T> handler) => handlers.Add(handler);

    public static void Unsubscribe(Action<T> handler)
    {
        int i = handlers.IndexOf(handler);
        if (i < 0) return;

        if (publishDepth > 0)
        {
            handlers[i] = null;   // no alteramos índices mientras se itera
            hasNulls = true;
        }
        else
        {
            handlers.RemoveAt(i);
        }
    }

    public static void Publish(in T ev)
    {
        publishDepth++;
        int count = handlers.Count; // los que se suscriban durante el Publish no se disparan en este
        for (int i = 0; i < count; i++)
        {
            try { handlers[i]?.Invoke(ev); }
            catch (Exception e) { Debug.LogException(e); }
        }
        publishDepth--;

        if (publishDepth == 0 && hasNulls)
        {
            handlers.RemoveAll(h => h == null);
            hasNulls = false;
        }
    }
}