using System.Collections;
using UnityEngine;

public class CanvasGroupTransitionEffect : TransitionEffect
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.35f;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public override IEnumerator FadeIn()
    {
        canvasGroup.blocksRaycasts = true;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public override IEnumerator FadeOut()
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}