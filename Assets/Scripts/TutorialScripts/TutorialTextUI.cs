using TMPro;
using UnityEngine;

public class TutorialTextUI : MonoBehaviour
{
    [SerializeField] private GameObject textContainer;
    [SerializeField] private TMP_Text tutorialText;

    private void Awake()
    {
        if (textContainer != null)
        {
            textContainer.SetActive(false);
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
        textContainer.SetActive(show);
    }
}
