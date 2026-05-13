using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocomotiveUpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private LocomotivesUpgradesButtonController buttonManager;
    public LocomotivesUpgradesButtonController ButtonManager {  get => buttonManager; set => buttonManager = value; }

    public Button button;

    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private string textDescription;

    private float upgrade;
    public float Upgrade { get => upgrade; set => upgrade = value; }


    void Start()
    {
        button = GetComponent<Button>();
    }

    public void DeactivateButton()
    {
        button.interactable = false;
        description.text = "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable == true)
        {
            //description.text = textDescription + "\n\n" + "Aumento de " + upgrade;
            description.text = textDescription;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        description.text = "";
    }
}
