using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    AudioSource source;
    Coroutine playingCoroutine;
    Sound currentSound; // NUEVO: guardamos referencia para poder reaplicar volumen si cambia en caliente

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play(Sound sound, float sfxVolumeMultiplier = 1f) // modificado
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }
        currentSound = sound;
        InitializeSound(sound, sfxVolumeMultiplier);
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

    public void InitializeSound(Sound data, float sfxVolumeMultiplier = 1f) // modificado
    {
        source.clip = data.clip;
        source.volume = data.volume * sfxVolumeMultiplier; // modificado
        source.loop = data.loop;
        source.pitch = data.pitch;
    }

    IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => source.isPlaying);
        AudioManager.Instance.ReturnToPool(this);
    }
}