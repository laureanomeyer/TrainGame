using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text spacialText;
    [SerializeField] private GameObject buttonsPanel;

    private void Start()
    {
        HideAll();
        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        spacialText.text = $"Estacion {sessionConfigRef.CurrentLevel}";
    }

    public void ShowText(string message)
    {
        textPanel.SetActive(true);
        buttonsPanel.SetActive(false);
        infoText.text = message;
    }


    public void ShowButtons()
    {
        textPanel.SetActive(false);
        buttonsPanel.SetActive(true);
    }

    public void HideAll()
    {
        textPanel.SetActive(false);
        buttonsPanel.SetActive(false);
    }

}