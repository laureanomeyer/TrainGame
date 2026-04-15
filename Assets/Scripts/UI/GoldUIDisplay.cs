using TMPro;
using UnityEngine;

public class GoldUIDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI GoldTextUI;

    public void UpdatedGold(float gold)
    {
        GoldTextUI.text = "Gold: " + gold;
    }
}
