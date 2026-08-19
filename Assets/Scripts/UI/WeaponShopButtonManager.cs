using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopButtonManager : MonoBehaviour
{
    [Header("Button from HUD")]
    [SerializeField] public GameObject[] shopButtons;
    private List<IShopButton> buttons = new List<IShopButton>();

    [Header("Level of progress")]
    [SerializeField] private int level;

    [Header("List of weapons in Stock")]
    [SerializeField] public WeaponInStocSO[] firtsSlotWeapons;
    [SerializeField] public WeaponInStocSO[] secondSlotWeapons;
    [SerializeField] public WeaponInStocSO[] thirdSlotWeapons;

    [SerializeField] private Button closeButton;
    [SerializeField] private InteractionZone interacZone;

    void Start()
    {
       PlayerBrain playerAtk = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBrain>();
       closeButton.onClick.AddListener(CloseButton);

       for (int i = 0; i < shopButtons.Length; i++)
       {
            buttons.Add(shopButtons[i].GetComponent<WeaponShopButton>()); 
       }



        buttons[0].WeaponInStock = firtsSlotWeapons;
       buttons[1].WeaponInStock = secondSlotWeapons;
       buttons[2].WeaponInStock = thirdSlotWeapons;

       foreach (IShopButton button in buttons)
       {
            button.PlayerReference = playerAtk;
            button.ButtonManager = this;

            button.SetValues(level);
       }
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(CloseButton);
    }

    private void CloseButton()
    {
        interacZone.DeactivateUI();
    }

    public void UpdateButtons(IShopButton button)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (button != buttons[i])
            {
                buttons[i].ActivateButton();
            }
        }
    }

    public bool TryConsumeGold(float amount)
    {
        return StoreManager.Instance.TrySpendGold(amount);
    }

    public float ShowPlayerGold()
    {
        return StoreManager.Instance.GetGold();
    }
}
