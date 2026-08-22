using System;
using System.Collections;
using UnityEngine;

public class WagonRenderController
{
    private WagonBrain wagonBrain;
    private Mesh wagonTopMesh;

    private int alphaPropertyID;

    private Coroutine fadeRoutine;
    private float currentTopAlpha = 1f;
    private const float fadeDuration = 0.1f;

    public WagonRenderController(WagonBrain brain)
    {
        wagonBrain = brain;

        if (wagonBrain.wagonTopMeshFilter != null)
        {
            wagonTopMesh = wagonBrain.wagonTopMeshFilter.mesh;
        }

        alphaPropertyID = Shader.PropertyToID("_Alpha");

        currentTopAlpha = 1f;
    }

    public void CheckWagonToChangeRender(bool canBreak)
    {
        if (canBreak) ChangeToDestroyWagon();
        else return;
    }

    private void ChangeToDestroyWagon()
    {
        if (fadeRoutine != null)
        {
            wagonBrain.StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        wagonBrain.SetDestroyed(true);

        if (wagonBrain.particles != null)
        {
            wagonBrain.particles.Play();
        }
    }

    public void ActivateWagonTop()
    {
        if (wagonBrain.wagonTopMeshFilter == null) return;
        if (wagonBrain.HPController != null && wagonBrain.HPController.IsBroken) return;

        wagonBrain.wagonTopMeshFilter.mesh = wagonTopMesh;
        StartFade(1f);
    }

    public void DeactivateWagonTop()
    {
        if (wagonBrain.wagonTopMeshFilter == null) return;
        if (wagonBrain.HPController != null && wagonBrain.HPController.IsBroken) return;

        StartFade(0f, () =>
        {
            wagonBrain.wagonTopMeshFilter.mesh = null;
        });
    }

    private void StartFade(float targetAlpha, Action onComplete = null)
    {
        if (fadeRoutine != null)
        {
            wagonBrain.StopCoroutine(fadeRoutine);
        }

        fadeRoutine = wagonBrain.StartCoroutine(FadeTopAlpha(currentTopAlpha, targetAlpha, onComplete));
    }

    private IEnumerator FadeTopAlpha(float from, float to, Action onComplete)
    {
        var rend = wagonBrain.wagonTopRender;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            currentTopAlpha = Mathf.Lerp(from, to, t / fadeDuration);

            wagonBrain.wagonTopRender.material.SetFloat(alphaPropertyID, currentTopAlpha);
            wagonBrain.wagonTopRender.material.GetFloat(alphaPropertyID);

            yield return null;
        }

        currentTopAlpha = to;
        wagonBrain.wagonTopRender.material.SetFloat(alphaPropertyID, to);

        fadeRoutine = null;
        onComplete?.Invoke();
    }
}
