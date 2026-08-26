using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopButtonManager : MonoBehaviour
{
    [Header("Button from HUD")]
    [SerializeField] public GameObject[] shopButtons;
    private List<IWeaponShopButton> buttons = new List<IWeaponShopButton>();

    [Header("Level of progress")]
    [SerializeField] private int level;

    [SerializeField] private Button closeButton;
    [SerializeField] private InteractionZone interacZone;

    void Start()
    {
       PlayerBrain playerAtk = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBrain>();
       closeButton.onClick.AddListener(CloseButton);

        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        level = sessionConfigRef.CurrentLevel;

       for (int i = 0; i < shopButtons.Length; i++)
       {
            buttons.Add(shopButtons[i].GetComponent<WeaponShopButton>()); 
       }

       foreach (IWeaponShopButton button in buttons)
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

    public void UpdateButtons(IWeaponShopButton button)
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
