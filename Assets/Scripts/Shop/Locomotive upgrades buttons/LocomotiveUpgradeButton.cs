using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocomotiveUpgradeButton : MonoBehaviour, IPointerEnterHandler
{
    private LocomotivesUpgradesButtonController buttonManager;
    private string defaultText;
    public LocomotivesUpgradesButtonController ButtonManager {  get => buttonManager; set => buttonManager = value; }

    public Button button;

    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private string textDescription;

    private float upgrade;
    public float Upgrade { get => upgrade; set => upgrade = value; }

    void Start()
    {
        defaultText = description.text;
    }

    public void DeactivateButton()
    {
        button.interactable = false;
        description.text = defaultText;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable == true)
        {
            description.text = textDescription;
        }
    }
}
