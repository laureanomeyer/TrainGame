using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WagonStoreManager : MonoBehaviour
{
    [Header("Wagon shop sections")]
    [SerializeField] private WagonShopButton[] shopButtons;

    [SerializeField] private DisplayTrain displayTrain;

    [SerializeField] public Button buyButton;
    [SerializeField] public Button rerollButton;
    [SerializeField] public Button closeButton;

    private int currentLevel;
    public int Level => currentLevel;

    //[SerializeField] public int maxLevel;

    [SerializeField] public TextMeshProUGUI nameTextUI;
    [SerializeField] public TextMeshProUGUI descriptionTextUI;
    [SerializeField] public TextMeshProUGUI priceTextUI;

    private void Start()
    {
        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        currentLevel = sessionConfigRef.CurrentLevel;

        foreach (var button in shopButtons)
        {
            button.Level = currentLevel;
            button.displayTrain = displayTrain;
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

    public void ActivateButtons()
    {
        buyButton.interactable = true;
        rerollButton.interactable = true;
    }

    public void DeactivateButtons()
    {
        buyButton.interactable = false;
        rerollButton.interactable = false;
    }

}
