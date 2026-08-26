using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

/// <summary>
/// Reproduce un conjunto de partículas (ParticleSystem legacy o VisualEffect/VFX Graph) con delay
/// configurable por entrada, o agrupadas con stagger automático. Se llama por id o por group desde
/// otros scripts del mismo objeto, Animation Events, o UnityEvents.
///
/// Para agregar una partícula nueva en el futuro: se arrastra la referencia en la lista del
/// Inspector, se le pone un id y (opcional) un group. No hace falta tocar este script.
/// </summary>
public class ParticleSequenceController : MonoBehaviour
{
    [Serializable]
    public class ParticleEntry
    {
        [Tooltip("Id único para llamar esta entrada por código (Play(\"id\")).")]
        public string id;

        [Tooltip("Grupo opcional. Permite reproducir varias entradas juntas con PlayGroup(), con stagger automático según su orden en la lista.")]
        public string group;

        [Header("Partícula (asignar solo una de las dos)")]
        [Tooltip("Sistema de partículas legacy.")]
        public ParticleSystem particle;

        [Tooltip("Efecto de VFX Graph.")]
        public VisualEffect visualEffect;

        [Header("Timing")]
        [Min(0f), Tooltip("Delay base en segundos antes de reproducir esta entrada. En PlayGroup() se suma al stagger del grupo.")]
        public float delay = 0f;

        [Tooltip("Si está activo, el delay ignora el Time.timeScale. Usalo si esto se dispara durante una cinemática con el juego pausado.")]
        public bool useUnscaledTime = false;

        [Header("Comportamiento")]
        [Tooltip("Si el GameObject de la partícula arranca desactivado, lo activa antes de reproducir.")]
        public bool activateObjectOnPlay = false;

        [Tooltip("Al hacer Stop, limpia lo ya emitido en vez de dejarlo terminar solo. Solo aplica a ParticleSystem.")]
        public bool clearOnStop = true;

        [Tooltip("Se dispara en el frame en que la partícula arranca. Enganchá acá el SFX u otra lógica sin tocar código.")]
        public UnityEvent onPlayed;
    }

    [Serializable]
    public class GroupSettings
    {
        [Tooltip("Nombre del grupo (debe matchear el campo 'group' de las entradas).")]
        public string group;

        [Min(0f), Tooltip("Segundos que se suman de delay entre cada entrada consecutiva del grupo, según su orden en la lista.")]
        public float staggerInterval = 0f;
    }

    [SerializeField] private List<ParticleEntry> particles = new();
    [SerializeField] private List<GroupSettings> groups = new();

    private Dictionary<string, ParticleEntry> _lookup;
    private Dictionary<string, float> _groupStagger;
    private Dictionary<string, Coroutine> _activeRoutines;
    private readonly Dictionary<float, WaitForSeconds> _waitCache = new();
    private readonly Dictionary<float, WaitForSecondsRealtime> _waitRealtimeCache = new();

    private void Awake()
    {
        _lookup = new Dictionary<string, ParticleEntry>(particles.Count);
        _activeRoutines = new Dictionary<string, Coroutine>(particles.Count);
        _groupStagger = new Dictionary<string, float>(groups.Count);

        foreach (var g in groups)
        {
            if (string.IsNullOrEmpty(g.group)) continue;
            _groupStagger[g.group] = g.staggerInterval;
        }

        foreach (var entry in particles)
        {
            if (string.IsNullOrEmpty(entry.id))
            {
                Debug.LogWarning($"[ParticleSequenceController] Entrada sin id en '{name}', se ignora.", this);
                continue;
            }

            if (entry.particle == null && entry.visualEffect == null)
            {
                Debug.LogWarning($"[ParticleSequenceController] Entrada '{entry.id}' sin ParticleSystem ni VisualEffect asignado en '{name}'.", this);
                continue;
            }

            if (!_lookup.TryAdd(entry.id, entry))
                Debug.LogWarning($"[ParticleSequenceController] Id duplicado '{entry.id}' en '{name}'.", this);
        }
    }

    // ---------- API pública ----------

    /// <summary>Reproduce una entrada por id, respetando su delay configurado.</summary>
    public void Play(string id)
    {
        if (!_lookup.TryGetValue(id, out var entry))
        {
            Debug.LogWarning($"[ParticleSequenceController] No existe partícula con id '{id}' en '{name}'.", this);
            return;
        }
        PlayEntry(entry);
    }

    /// <summary>Reproduce todas las entradas configuradas, cada una con su propio delay individual.</summary>
    public void PlayAll()
    {
        foreach (var entry in particles)
            PlayEntry(entry);
    }

    /// <summary>
    /// Reproduce todas las entradas de un grupo, aplicando stagger automático según su orden
    /// en la lista y el staggerInterval configurado para ese grupo. Agregar una partícula nueva
    /// al grupo (en el Inspector) no requiere ningún cambio de código.
    /// </summary>
    public void PlayGroup(string group)
    {
        if (string.IsNullOrEmpty(group)) return;

        float stagger = _groupStagger.TryGetValue(group, out var interval) ? interval : 0f;
        int index = 0;

        foreach (var entry in particles)
        {
            if (entry.group != group) continue;

            float effectiveDelay = entry.delay + index * stagger;
            PlayEntry(entry, effectiveDelay);
            index++;
        }
    }

    /// <summary>Detiene una entrada puntual (cancela su delay pendiente si lo tenía).</summary>
    public void Stop(string id)
    {
        if (!_lookup.TryGetValue(id, out var entry)) return;
        StopEntry(entry);
    }

    /// <summary>Detiene todas las entradas de un grupo.</summary>
    public void StopGroup(string group)
    {
        foreach (var entry in particles)
            if (entry.group == group) StopEntry(entry);
    }

    /// <summary>Detiene todo lo que esté sonando o pendiente de disparar.</summary>
    public void StopAll()
    {
        foreach (var entry in particles)
            StopEntry(entry);
    }

    /// <summary>True si la entrada está actualmente emitiendo.</summary>
    public bool IsPlaying(string id)
    {
        if (!_lookup.TryGetValue(id, out var entry)) return false;
        if (entry.particle != null) return entry.particle.isPlaying;
        if (entry.visualEffect != null) return entry.visualEffect.HasAnySystemAwake();
        return false;
    }

    // ---------- Internals ----------

    private void PlayEntry(ParticleEntry entry, float? overrideDelay = null)
    {
        if (entry == null || (entry.particle == null && entry.visualEffect == null)) return;

        // Si ya había un delay corriendo para esta entrada, se cancela y se reinicia.
        if (_activeRoutines.TryGetValue(entry.id, out var running) && running != null)
            StopCoroutine(running);

        float delay = overrideDelay ?? entry.delay;

        if (delay <= 0f)
        {
            FireEntry(entry);
            return;
        }

        _activeRoutines[entry.id] = StartCoroutine(PlayDelayed(entry, delay));
    }

    private IEnumerator PlayDelayed(ParticleEntry entry, float delay)
    {
        yield return GetWait(delay, entry.useUnscaledTime);
        FireEntry(entry);
        _activeRoutines.Remove(entry.id);
    }

    private void FireEntry(ParticleEntry entry)
    {
        if (entry.activateObjectOnPlay)
        {
            if (entry.particle != null) entry.particle.gameObject.SetActive(true);
            else if (entry.visualEffect != null) entry.visualEffect.gameObject.SetActive(true);
        }

        if (entry.particle != null) entry.particle.Play(true);
        else if (entry.visualEffect != null) entry.visualEffect.Play();

        entry.onPlayed?.Invoke();
    }

    private void StopEntry(ParticleEntry entry)
    {
        if (entry == null) return;

        if (_activeRoutines.TryGetValue(entry.id, out var routine) && routine != null)
        {
            StopCoroutine(routine);
            _activeRoutines.Remove(entry.id);
        }

        if (entry.particle != null)
        {
            var behaviour = entry.clearOnStop
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting;
            entry.particle.Stop(true, behaviour);
        }
        else if (entry.visualEffect != null)
        {
            entry.visualEffect.Stop();
        }
    }

    private object GetWait(float seconds, bool unscaled)
    {
        if (unscaled)
        {
            if (!_waitRealtimeCache.TryGetValue(seconds, out var waitRt))
            {
                waitRt = new WaitForSecondsRealtime(seconds);
                _waitRealtimeCache[seconds] = waitRt;
            }
            return waitRt;
        }

        if (!_waitCache.TryGetValue(seconds, out var wait))
        {
            wait = new WaitForSeconds(seconds);
            _waitCache[seconds] = wait;
        }
        return wait;
    }
}