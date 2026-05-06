using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
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

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        stationText.text = "Cargando...";

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

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

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
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = stationText.color;
        color.a = alpha;
        stationText.color = color;
    }

    private void HideTransitionInstantly()
    {
        SetFadeAlpha(0f);
        SetTextAlpha(0f);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}