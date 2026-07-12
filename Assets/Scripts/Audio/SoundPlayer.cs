using System.Collections;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public bool isPlaying;
    public SoundPlayer()
    {
        isPlaying = false;
    }


    public void PlaySound(Sound soundToPlay, float duration)
    {
        StartCoroutine(Play_Coroutine(soundToPlay, duration));
    }
    public IEnumerator Play_Coroutine(Sound soundToPlay, float duration)
    {
        isPlaying = true;
        soundToPlay.source.Play();
        yield return new WaitForSeconds(duration);
        isPlaying = false;
    }
}
