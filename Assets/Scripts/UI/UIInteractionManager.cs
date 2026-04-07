using UnityEngine;
using TMPro;

public class InteractionUIManager : MonoBehaviour
{

    [SerializeField] private GameObject textPanel;
    [SerializeField] private TMP_Text infoText;

    [SerializeField] private GameObject buttonsPanel;

    private void Start()
    {
        HideAll();
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