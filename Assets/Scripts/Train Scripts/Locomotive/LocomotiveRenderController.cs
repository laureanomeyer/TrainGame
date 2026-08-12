using System;
using System.Collections;
using UnityEngine;

public class LocomotiveRenderController
{
    private LocomotiveBrain locomotiveBrain;
    private Mesh locomotiveTopMesh;

    private int alphaPropertyID;

    private Coroutine fadeRoutine;
    private float currentTopAlpha = 1f;
    private const float fadeDuration = 0.25f;

    public LocomotiveRenderController(LocomotiveBrain locomotiveBrain)
    {
        this.locomotiveBrain = locomotiveBrain;

        if (this.locomotiveBrain.locomotiveTopMeshFilter != null)
        {
            locomotiveTopMesh = this.locomotiveBrain.locomotiveTopMeshFilter.mesh;
        }

        alphaPropertyID = Shader.PropertyToID("_Alpha");

        currentTopAlpha = 0f;
    }

    public void ActivateWagonTop()
    {
        if (locomotiveBrain.locomotiveTopMeshFilter == null) return;

        locomotiveBrain.locomotiveTopMeshFilter.mesh = locomotiveTopMesh;
        StartFade(1f);
    }

    public void DeactivateWagonTop()
    {
        if (locomotiveBrain.locomotiveTopMeshFilter == null) return;

        StartFade(0f, () =>
        {
            locomotiveBrain.locomotiveTopMeshFilter.mesh = null;
        });
    }

    public void ForceDeactivateTop()
    {
        if (locomotiveBrain.locomotiveTopMeshFilter == null) return;
    }

    private void StartFade(float targetAlpha, Action onComplete = null)
    {
        if (fadeRoutine != null)
        {
            locomotiveBrain.StopCoroutine(fadeRoutine);
            locomotiveBrain.locomotiveTopRender.material.SetFloat(alphaPropertyID, 0f);
        }

        fadeRoutine = locomotiveBrain.StartCoroutine(FadeTopAlpha(currentTopAlpha, targetAlpha, onComplete));
    }

    private IEnumerator FadeTopAlpha(float from, float to, Action onComplete)
    {
        var rend = locomotiveBrain.locomotiveTopRender;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            currentTopAlpha = Mathf.Lerp(from, to, t / fadeDuration);

            locomotiveBrain.locomotiveTopRender.material.SetFloat(alphaPropertyID, currentTopAlpha);
            locomotiveBrain.locomotiveTopRender.material.GetFloat(alphaPropertyID);

            yield return null;
        }

        currentTopAlpha = to;
        locomotiveBrain.locomotiveTopRender.material.SetFloat(alphaPropertyID, to);

        fadeRoutine = null;
        onComplete?.Invoke();
    }
}
