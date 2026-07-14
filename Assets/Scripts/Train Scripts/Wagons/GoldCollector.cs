using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GoldCollector
{
    private WagonHP wagonHP;

    private float gold;
    public float Gold => gold;

    private float storageCapacity;

    private TextMeshProUGUI goldDisplayUI;

    private float originalFontSize;
    private float maxFontSize = 16f;

    private float duration = 0.3f;

    private CancellationTokenSource cts;

    private Action<float, float> setCoinsModels;
    private StatSystem statsRef;

    public GoldCollector(WagonHP hpController, TextMeshProUGUI CurrentGoldUI, float collectorStorageCapacity, Action<float, float> action)
    {
        wagonHP = hpController;
        goldDisplayUI = CurrentGoldUI;
        originalFontSize = goldDisplayUI.fontSize;
        storageCapacity = collectorStorageCapacity;
        EventBus.Subscribe<OnGoldEarnedEvent>(CollectGold);
        this.setCoinsModels = action;
        statsRef = ServiceLocator.Get<StatSystem>();
    }

    public void ActivateOnDestroy()
    {
        EventBus.Unsubscribe<OnGoldEarnedEvent>(CollectGold);
        cts?.Cancel();
        cts?.Dispose();
    }

    public void CollectGold(OnGoldEarnedEvent goldEvent)
    {
        if (wagonHP.IsBroken == false)
        {
            gold += goldEvent.Amount * statsRef.GetLocoMultiplier(StatType.GoldMultiplier);

            setCoinsModels(gold, storageCapacity);
            goldDisplayUI.text = "$" + gold;
            PlayScaleEffect();
        }
        else
        {
            EmptyGold();
        }
    }

    public float GiveGold()
    {
        if (wagonHP.IsBroken == false)
        {
            float goldToGive = gold;
            EmptyGold();
            
            if (goldToGive > 0)
            {
                EventBus.Publish(new OnTakeGoldEvent());
            }

            return goldToGive;
        }
        else
        {
            return 0;
        }
    }

    public void EmptyGold()
    {
        gold = 0;
        setCoinsModels(gold, storageCapacity);
        goldDisplayUI.text = string.Empty;
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

            goldDisplayUI.fontSize = Mathf.Lerp(from, to, t);

            await Task.Yield();
        }

        goldDisplayUI.fontSize = to;
    }
}
