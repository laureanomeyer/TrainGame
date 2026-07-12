using System.Collections;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    AudioSource source;
    Coroutine playingCoroutine;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play(Sound sound)
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }
        InitializeSound(sound);
        source.Play();
        playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }
        source.Stop();
        AudioManager.Instance.ReturnToPool(this);
    }

    public void InitializeSound(Sound data)
    {
        source.clip = data.clip;
        source.volume = data.volume;
        source.loop = data.loop;
        source.pitch = data.pitch;
    }

    IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => source.isPlaying);
        AudioManager.Instance.ReturnToPool(this);
    }
}
