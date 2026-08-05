using System.Collections;
using UnityEngine;

public class RunDefeatUI : MonoBehaviour
{
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private float barTargetHeight = 90f;
    [SerializeField] private float letterBoxDuration = 1.3f;

    private void OnEnable() => EventBus.Subscribe<OnRunEndedEvent>(OnRunEnded);
    private void OnDisable() => EventBus.Unsubscribe<OnRunEndedEvent>(OnRunEnded);

    private void OnRunEnded(OnRunEndedEvent evt)
    {
        if (evt.Result != RunResult.Defeat) return;
        
        EventBus.Publish(new OnActivateUiEvent(false));
        StartCoroutine(AnimateLetterBox(0f, barTargetHeight));
    }

    private IEnumerator AnimateLetterBox(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < letterBoxDuration)
        {
            elapsed += Time.deltaTime;
            float h = Mathf.Lerp(from, to, elapsed / letterBoxDuration);
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, h);
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, h);
            yield return null;
        }
    }
}
