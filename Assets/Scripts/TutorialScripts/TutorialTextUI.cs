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
        TutorialEvents.OnSetTutorialText += SetText;
        TutorialEvents.OnSetTutorialVisible += SetVisible;
    }

    private void OnDisable()
    {
        TutorialEvents.OnSetTutorialText -= SetText;
        TutorialEvents.OnSetTutorialVisible -= SetVisible;
    }

    private void SetText(string text)
    {
        tutorialText.text = text;
    }
    private void SetVisible(bool show)
    {
        textContainer.SetActive(show);
    }
}
