using TMPro;
using DG.Tweening;
using UnityEngine;

public class TutorialTextUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup textContainer;
    [SerializeField] private TMP_Text tutorialText;

    private Vector3 initialScale;

    private void Awake()
    {
        if (textContainer != null)
        {
            textContainer.alpha = 0f;
            initialScale = textContainer.transform.localScale;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnSetTutorialTextEvent>(CallSetTextEvent);
        EventBus.Subscribe<OnSetTutorialVisibleEvent>(CallSetVisibleTutoriaLEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnSetTutorialTextEvent>(CallSetTextEvent);
        EventBus.Unsubscribe<OnSetTutorialVisibleEvent>(CallSetVisibleTutoriaLEvent);
    }

    private void CallSetTextEvent(OnSetTutorialTextEvent setTextEvent)
    {
        SetText(setTextEvent.Text);
    }

    private void SetText(string text)
    {
        tutorialText.text = text;
    }

    private void CallSetVisibleTutoriaLEvent(OnSetTutorialVisibleEvent tutorialVisibleEvent)
    {
        SetVisible(tutorialVisibleEvent.Show);
    }

    private void SetVisible(bool show)
    {
        if (show)
        {
            textContainer.alpha = 1f;
            textContainer.transform.DOScale(initialScale, 0.5f).SetEase(Ease.OutBounce);
        }

        else
        {
            textContainer.alpha = 0f;
            textContainer.transform.DOScale(0.8f, 0.2f);
        }

    }
}
