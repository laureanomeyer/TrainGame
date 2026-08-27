using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CoalCollector
{
    private float coal;
    public float Coal => coal;

    private float storageCapacity;

    private TextMeshProUGUI coalDisplayUI;

    private float originalFontSize;
    private float maxFontSize = 16f;

    private float duration = 0.3f;

    private CancellationTokenSource cts;

    private Action<float, float> setCoalModels;

    public CoalCollector(TextMeshProUGUI CurrentCoalUI)
    {
        coalDisplayUI = CurrentCoalUI;
        originalFontSize = coalDisplayUI.fontSize;
        EventBus.Subscribe<OnCoalEarnedEvent>(CollectCoal);

        setCoalModels(coal,storageCapacity);
    }

    public void ActivateOnDestroy()
    {
        EventBus.Unsubscribe<OnCoalEarnedEvent>(CollectCoal);
        cts?.Cancel();
        cts?.Dispose();
    }

    public void CollectCoal(OnCoalEarnedEvent coalEvent)
    {
        coal +=1;

        setCoalModels(coal, storageCapacity);
        coalDisplayUI.text = "$" + coal;
        PlayScaleEffect();
        EmptyCoal();

    }

    public void GiveCoal()
    {
        EmptyCoal();
        EventBus.Publish(new OnTakeCoalEvent());
    }

    public void EmptyCoal()
    {
        coal = -1;
        setCoalModels(coal, storageCapacity);
        coalDisplayUI.text = string.Empty;
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
