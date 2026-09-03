using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CoalCollector
{
    private float coal = 1;
    public float Coal => coal;

    public bool HasCoal => coal > 0;

    private TextMeshProUGUI coalDisplayUI;

    private float originalFontSize;
    private float maxFontSize = 16f;

    private float duration = 0.3f;

    private CancellationTokenSource cts;


    public CoalCollector(TextMeshProUGUI CurrentCoalUI)
    {
        
        coalDisplayUI = CurrentCoalUI;
        originalFontSize = coalDisplayUI.fontSize;
        EventBus.Subscribe<OnCoalEarnedEvent>(GainCoal);
        EventBus.Subscribe<OnTakeCoalEvent>(GiveCoal);


    }

    public void ActivateOnDestroy()
    {
        EventBus.Unsubscribe<OnCoalEarnedEvent>(GainCoal);
        EventBus.Unsubscribe<OnTakeCoalEvent>(GiveCoal);

        cts?.Cancel();
        cts?.Dispose();
    }

    public void GainCoal(OnCoalEarnedEvent coalEvent)
    {
        coal +=1;

        coalDisplayUI.text = coal.ToString();
        PlayScaleEffect();

    }

    public void GiveCoal(OnTakeCoalEvent coalGivenEvent)
    {
        Debug.Log("coal given");
        EmptyCoal();
    }

    public void EmptyCoal()
    {
        if (coal > 0)
        {
            coal -=1;
        }
        coalDisplayUI.text = coal.ToString();
    }

    private async void PlayScaleEffect()
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;
        try
        {
            await AnimateFontSize(originalFontSize, maxFontSize, duration, token);
            await AnimateFontSize(maxFontSize, originalFontSize, duration, token);
        }
        catch (OperationCanceledException) { }
    }

    private async Task AnimateFontSize(float from, float to, float dur, CancellationToken token)
    {
        float elapsed = 0f;

        while (elapsed < dur)
        {
            token.ThrowIfCancellationRequested();

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            t = t * t * (3f - 2f * t);

            coalDisplayUI.fontSize = Mathf.Lerp(from, to, t);

            await Task.Yield();
        }

        coalDisplayUI.fontSize = to;
    }
}
