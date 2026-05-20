using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WagonStoreManager : MonoBehaviour
{
    [Header("Wagon shop sections")]
    [SerializeField] private WagonShopButton[] shopButtons;

    [SerializeField] public Button buyButton;
    [SerializeField] public Button rerollButton;
    [SerializeField] private GameObject wagonStoreUI;

    private int currentLevel;
    public int Level => currentLevel;

    //[SerializeField] public int maxLevel;

    [SerializeField] public TextMeshProUGUI descriptionTextUI;

    private void Start()
    {
        currentLevel = GameManager.Instance.Session.SessionConfig.CurrentLevel;

        foreach (var button in shopButtons)
        {
            button.Level = currentLevel;
        }
    }

    public bool TryConsumeGold(float amount)
    {
        return StoreManager.Instance.TrySpendGold(amount);
    }
    public float GetPlayerGold()
    {
        return StoreManager.Instance.GetGold();
    }

    public void DeactivateReroll()
    {
        rerollButton.interactable = false;
    }

    public void DeactivateUI()
    {
        wagonStoreUI.SetActive(false);
    }

    public void ActivateUI()
    {
        wagonStoreUI.SetActive(true);
    }
}
