using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{

    public static SceneTransitionManager Instance;

    [Header("Transition Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private TMP_Text stationText;
    [SerializeField] private Canvas canvas;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float textFadeDuration = 1.5f;

    public bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetFadeAlpha(0f);
        SetTextAlpha(0f);

        canvas.sortingOrder = -1;

    }

    public void TransitionToScene(string sceneName, string stationName)
    {
        if (isTransitioning) return;
        
        StartCoroutine(TransitionCoroutine(sceneName, stationName));
    }

    private IEnumerator TransitionCoroutine(string sceneName, string stationName)
    {
        canvas.sortingOrder = 100;

        isTransitioning = true;

        stationText.text = stationName;

        yield return FadeImage(0f, 1f);
        yield return FadeText(0f, 1f);

        yield return new WaitForSeconds(textFadeDuration);

        yield return FadeText(1f, 0f);
        
        SceneManager.LoadScene(sceneName);

        yield return null;

        yield return FadeImage(1f,0f);

        canvas.sortingOrder = -1;

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
            timer += Time.deltaTime;
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
            timer += Time.deltaTime;
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

}
