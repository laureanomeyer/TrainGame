using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] Sound[] sounds;
    public static AudioManager Instance;
    private List<SoundPlayer> players = new List<SoundPlayer>();

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
        }

        //consisten Sound through-out Scenes
        DontDestroyOnLoad(gameObject);

        //setting sounds in AudioSources
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        players.Add(new SoundPlayer());
      /*  players.Add(new SoundPlayer());
        players.Add(new SoundPlayer());
        players.Add(new SoundPlayer());
        players.Add(new SoundPlayer());
        players.Add(new SoundPlayer());
        players.Add(new SoundPlayer());
        players.Add(new SoundPlayer());*/
    }

    public void Play(string name, float duration = 0)
    {
        Play_Coroutine(name, duration);
    }

    public void Play(Sound sound)
    {
        if (sound != null)
            StartCoroutine(Play_Coroutine(sound));
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
    ///////Couroutines Function///////
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

    void Play_Coroutine(string name, float duration)
    {

        for (int i = 0; i <= players.Count; i++)
        {
            if (!players[i].isPlaying)
            {
                var s = Array.Find(sounds, sound => sound.name == name);
                players[i].PlaySound( s, duration);
                break;
            }
        }
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
        if(s.source != null) return;

        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;

    }
}