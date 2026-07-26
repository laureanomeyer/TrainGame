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
    private const float fadeDuration = 0.25f;

    public WagonRenderController(WagonBrain brain)
    {
        wagonBrain = brain;

        if (wagonBrain.topMeshFilterWagon != null)
        {
            wagonTopMesh = wagonBrain.topMeshFilterWagon.mesh;
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

        if (wagonBrain.floorMeshFilterWagon != null)
        {
            wagonBrain.floorMeshFilterWagon.mesh = wagonBrain.floorMeshDestroyWagon;
            wagonBrain.floorRenderWagon.material = wagonBrain.destroyWagonMaterial;
        }

        if (wagonBrain.bodyMeshFilterWagon != null)
        {
            wagonBrain.bodyMeshFilterWagon.mesh = wagonBrain.bodyMeshDestroyWagon;
            wagonBrain.bodyRenderWagon.material = wagonBrain.destroyWagonMaterial;
        }

        if (wagonBrain.topMeshFilterWagon != null)
        {
            wagonBrain.topMeshFilterWagon.mesh = null;
        }

        if (wagonBrain.particles != null)
        {
            wagonBrain.particles.Play();
        }
    }

    private void ChangeToDestroyWagonFloor()
    {
        if (wagonBrain.floorMeshFilterWagon != null)
        {
            wagonBrain.floorMeshFilterWagon.mesh = wagonBrain.floorMeshDestroyWagon;
            wagonBrain.floorRenderWagon.material = wagonBrain.destroyWagonMaterial;
        }
    }

    public void SetWagonMeshAndMaterial(Mesh floor, Mesh body)
    {
        wagonBrain.floorMeshFilterWagon.mesh = floor;
        wagonBrain.bodyMeshFilterWagon.mesh = body;

        wagonBrain.floorRenderWagon.material = wagonBrain.destroyWagonMaterial;
        wagonBrain.bodyRenderWagon.material = wagonBrain.destroyWagonMaterial;
    }

    public void ActivateWagonTop()
    {
        if (wagonBrain.topMeshFilterWagon == null) return;
        if (wagonBrain.HPController != null && wagonBrain.HPController.IsBroken) return;

        wagonBrain.topMeshFilterWagon.mesh = wagonTopMesh;
        StartFade(1f);
    }

    public void DeactivateWagonTop()
    {
        if (wagonBrain.topMeshFilterWagon == null) return;
        if (wagonBrain.HPController != null && wagonBrain.HPController.IsBroken) return;

        StartFade(0f, () =>
        {
            wagonBrain.topMeshFilterWagon.mesh = null;
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
        var rend = wagonBrain.topRenderWagon;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            currentTopAlpha = Mathf.Lerp(from, to, t / fadeDuration);

            wagonBrain.topRenderWagon.material.SetFloat(alphaPropertyID, currentTopAlpha);
            wagonBrain.topRenderWagon.material.GetFloat(alphaPropertyID);
            Debug.Log(wagonBrain.topRenderWagon.material.GetFloat(alphaPropertyID));

            yield return null;
        }

        currentTopAlpha = to;
        wagonBrain.topRenderWagon.material.SetFloat(alphaPropertyID, to);

        fadeRoutine = null;
        onComplete?.Invoke();
    }
}
