using TMPro;
using UnityEngine;

public class WagonStoreManager : MonoBehaviour
{
    [Header("Level of progress")]
    [SerializeField] private int level;
    public int Level => level;

    [Header("MoneyManager reference")]
    [SerializeField] private MoneyManager moneyManager;

    [SerializeField] public TextMeshProUGUI descriptionTextUI;

    public void ConsumeGold(float amount)
    {
        moneyManager.ConsumePlayerGold(amount);
    }

    public float ShowPlayerGold()
    {
        return moneyManager.ShowPlayerGold();
    }

    public float ShowGold()
    {
        return moneyManager.PlayerMoney;
    }
}
