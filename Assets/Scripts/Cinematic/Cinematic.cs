using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class Cinematic : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector cine;

    [Header("Objects")]
    [SerializeField] private GameObject cineObj;

    [Header("Cameras")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Camera cinematicCamera;

    [Header("Dynamic Camera Start")]
    [SerializeField] private Transform cinematicRoot;
    [SerializeField] private Transform animatedCameraTransform;

    [Header("Fade Timing")]
    [SerializeField] private float fadeStartBeforeCinematicEnds = 1f;

    [Header("UI")]
    [SerializeField] private GameObject[] gameplayUIElements;

    private bool isPlaying;
    private bool runFinishedCalled;

    private void Awake()
    {
        if (cineObj != null)
            cineObj.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.enabled = true;

        if (cinematicCamera != null)
            cinematicCamera.enabled = false;

        if (cine != null)
        {
            cine.playOnAwake = false;
            cine.Stop();
            cine.time = 0;
        }
    }

    private void OnEnable()
    {
        if (cine != null)
            cine.stopped += OnCinematicFinished;
    }

    private void OnDisable()
    {
        if (cine != null)
            cine.stopped -= OnCinematicFinished;
    }

    public void PlayCinematic()
    {
        if (isPlaying) return;

        isPlaying = true;
        runFinishedCalled = false;

        SetGameplayUI(false);

        if (cineObj != null)
            cineObj.SetActive(true);

        SetCinematicStartFromGameplayCamera();

        if (gameplayCamera != null)
            gameplayCamera.enabled = false;

        if (cinematicCamera != null)
            cinematicCamera.enabled = true;

        cine.time = 0;
        cine.Play();

        StartCoroutine(StartFadeBeforeTimelineEnds());
    }

    private void SetCinematicStartFromGameplayCamera()
    {
        if (gameplayCamera == null) return;

        if (cinematicRoot != null)
        {
            cinematicRoot.position = gameplayCamera.transform.position;
            cinematicRoot.rotation = gameplayCamera.transform.rotation;
        }

        if (animatedCameraTransform != null)
        {
            animatedCameraTransform.localPosition = Vector3.zero;
            animatedCameraTransform.localRotation = Quaternion.identity;
        }
    }

    private IEnumerator StartFadeBeforeTimelineEnds()
    {
        float timelineDuration = (float)cine.duration;
        float waitTime = timelineDuration - fadeStartBeforeCinematicEnds;

        if (waitTime < 0f)
            waitTime = 0f;

        yield return new WaitForSeconds(waitTime);

        if (runFinishedCalled) yield break;

        runFinishedCalled = true;

        RunManager.Instance.OnRunFinished();
    }

    private void OnCinematicFinished(PlayableDirector director)
    {
        isPlaying = false;

        // IMPORTANTE:
        // No vuelvo a la cámara del player acá.
        // No apago la cámara cinemática acá.
        // No apago cineObj acá.
        //
        // La pantalla ya debería estar en fade negro,
        // y SceneTransitionManager va a cargar la próxima escena.

        if (!runFinishedCalled)
        {
            runFinishedCalled = true;
            RunManager.Instance.OnRunFinished();
        }
    }

    private void SetGameplayUI(bool value)
    {
        for (int i = 0; i < gameplayUIElements.Length; i++)
        {
            if(gameplayUIElements[i] != null)
            {
                gameplayUIElements[i].gameObject.SetActive(value);
            }
        }
    }
}