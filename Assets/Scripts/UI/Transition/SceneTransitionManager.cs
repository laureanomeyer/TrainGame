using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private GameObject transitionRootCanvas;
    [SerializeField] private Image fadeImage;
    [SerializeField] private TMP_Text stationText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float textFadeDuration = 1.5f;
    [SerializeField] private float textStayDuration = 1f;

    private bool isTransitioning;

    private void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        HideTransitionInstantly();
    }

    public void TransitionToScene(string sceneName, SceneTransitionType transitionType)
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionCoroutine(sceneName, transitionType));
    }

    private IEnumerator TransitionCoroutine(string sceneName, SceneTransitionType transitionType)
    {
        isTransitioning = true;

        if (transitionRootCanvas != null)
        {
            transitionRootCanvas.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
        }

        if (stationText != null)
        {
            stationText.raycastTarget = true;
        }

        stationText.text = GetTransitionText(transitionType);

        yield return FadeImage(0f, 1f);
        yield return FadeText(0f, 1f);

        yield return new WaitForSecondsRealtime(textStayDuration);

        yield return FadeText(1f, 0f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return FadeImage(1f, 0f);

        HideTransitionInstantly();

        isTransitioning = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishSceneChange();
        }
    }

    private IEnumerator FadeImage(float from, float to)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeDuration;

            SetFadeAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetFadeAlpha(to);
    }

    private IEnumerator FadeText(float from, float to)
    {
        float timer = 0f;

        while (timer < textFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / textFadeDuration;

            SetTextAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetTextAlpha(to);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void SetTextAlpha(float alpha)
    {
        if (stationText == null) return;

        Color color = stationText.color;
        color.a = alpha;
        stationText.color = color;
    }

    private string GetTransitionText(SceneTransitionType transitionType)
    {
        int currentLevel = 1;

        if (GameManager.Instance != null &&
            GameManager.Instance.Session != null &&
            GameManager.Instance.Session.SessionConfig != null)
        {
            currentLevel = GameManager.Instance.Session.SessionConfig.CurrentLevel;
        }

        switch (transitionType)
        {
            case SceneTransitionType.StartingRun:
                return $"Comenzando trayecto {currentLevel}";

            case SceneTransitionType.EndingRun:
                return $"Llegando a la estación {currentLevel}";

            case SceneTransitionType.MainMenu:
                return "Volviendo al menú principal";

            case SceneTransitionType.Final:
                return "Finalizando viaje";

            default:
                return "Cargando...";
        }
    }

    private void HideTransitionInstantly()
    {
        SetFadeAlpha(0f);
        SetTextAlpha(0f);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
        }

        if (stationText != null)
        {
            stationText.raycastTarget = false;
        }

        if (transitionRootCanvas != null)
        {
            transitionRootCanvas.SetActive(false);
        }
    }
}