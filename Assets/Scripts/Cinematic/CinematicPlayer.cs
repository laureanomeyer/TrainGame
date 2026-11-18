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
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += _ => videoPlayer.Play();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null) videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= skipLockTime && Keyboard.current.spaceKey.wasPressedThisFrame)
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