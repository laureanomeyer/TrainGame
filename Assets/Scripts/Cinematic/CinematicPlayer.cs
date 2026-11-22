using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class CinematicPlayer : MonoBehaviour
{
    private enum CinematicType { Intro, Victory }

    [SerializeField] private CinematicType type;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float skipLockTime = 2f;

    private bool finished;
    private float timer;

    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnPrepared;
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        float volume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        for (ushort i = 0; i < vp.audioTrackCount; i++)
            vp.SetDirectAudioVolume(i, volume);

        vp.Play();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= skipLockTime && Keyboard.current.spaceKey.wasPressedThisFrame)
            Finish();
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        if (timer < skipLockTime) return;

        Finish();
    }
    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;
        if (timer < skipLockTime) return;

        Finish();
    }

    private void OnVideoFinished(VideoPlayer vp) => Finish();

    private void Finish()
    {
        if (finished) return;
        finished = true;

        if (type == CinematicType.Intro) GameManager.Instance.FinishIntroCinematic();
        else GameManager.Instance.FinishVictoryCinematic();
    }
}