using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] Sound[] sounds;
    public static AudioManager Instance;
    IObjectPool<SoundPlayer> soundPlayerPool;
    readonly List<SoundPlayer> activeSoundPlayers = new List<SoundPlayer>();
    public readonly Dictionary<Sound, int> Counts = new Dictionary<Sound, int>();

    [SerializeField] private SoundPlayer soundPlayerPrefab;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private int maxSoundInstances = 30;

    public float SFXVolume { get; private set; } = 1f;

    private void Awake()
    {
        //Singelton Pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //consisten Sound through-out Scenes
        DontDestroyOnLoad(gameObject);

        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        //setting sounds in AudioSources
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        InitializePool();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void Play(string name)
    {
        SoundPlayer emitter = soundPlayerPool.Get();

        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null) emitter.Play(s, SFXVolume);
    }

    public void Play(Sound sound)
    {
        if (sound != null)
            StartCoroutine(Play_Coroutine(sound));
    }

    public void PlayOnScreen(string name, bool isOnScreen)
    {
        if (isOnScreen)
        {
            SoundPlayer emitter = soundPlayerPool.Get();

            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s != null) emitter.Play(s, SFXVolume);
        }

    }

    public void ChangeVolume(string name, float changed, float duration = 0)
    {
        StartCoroutine(ChangeVolume_Coroutine(name, changed, duration));
    }
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.Stop();
    }
    public void Stop(Sound sound)
    {
        if (sound != null)
            sound.source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.Pause();
    }

    public void Resume(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.UnPause();
    }
    IEnumerator ChangeVolume_Coroutine(string name, float changed, float duration)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            yield return null;
        else
        {
            s.source.volume = changed;
            yield return new WaitForSeconds(duration);

            ///<summary>
            ///Set the volume back to original if duration == 0
            /// </summary>
            if (duration > 0)
            {
                s.source.volume = s.volume;
            }
        }
    }

    public AudioSource GetSource(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s?.source;
    }

    public bool IsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s != null && s.source.isPlaying;
    }

    public void SetPitch(string name, float pitch)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
            s.source.pitch = pitch;
    }

    public void SetVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
            s.source.volume = volume;
    }

    IEnumerator Play_Coroutine(Sound soundToPlay)
    {
        if (soundToPlay == null)
            yield return null;
        soundToPlay.source.Play();
        yield return new WaitForSeconds(1f);
    }

    public void InitializeSound(Sound s)
    {
        if (s.source != null) return;

        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
    }

    #region Object Pool
    void InitializePool()
    {
        soundPlayerPool = new ObjectPool<SoundPlayer>(
            CreateSoundPlayer,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyPool,
            collectionCheck,
            defaultCapacity,
            maxPoolSize);

        for (int i = 0; i <= defaultCapacity; i++)
        {
            soundPlayerPool.Release(CreateSoundPlayer());
        }
    }

    public SoundPlayer Get()
    {
        return soundPlayerPool.Get();
    }

    public void ReturnToPool(SoundPlayer soundPlayer)
    {
        soundPlayerPool.Release(soundPlayer);
    }

    public bool CanPlaySound(Sound sound)
    {
        return !Counts.TryGetValue(sound, out var count) || count < maxSoundInstances;
    }
    SoundPlayer CreateSoundPlayer()
    {
        var soundPlayer = Instantiate(soundPlayerPrefab, this.transform);
        soundPlayer.gameObject.SetActive(false);
        return soundPlayer;
    }

    void OnTakeFromPool(SoundPlayer soundPlayer)
    {
        soundPlayer.gameObject.SetActive(true);
        activeSoundPlayers.Add(soundPlayer);
    }
    void OnReturnToPool(SoundPlayer soundPlayer)
    {
        soundPlayer.gameObject.SetActive(false);
        activeSoundPlayers.Remove(soundPlayer);
    }
    void OnDestroyPool(SoundPlayer soundPlayer)
    {
        Destroy(soundPlayer.gameObject);
    }
    #endregion
}