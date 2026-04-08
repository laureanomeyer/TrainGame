using NUnit.Framework;
using System;
using UnityEngine;

public class WeaponShopButtonManager : MonoBehaviour
{
    [SerializeField] public IShopButton[] uttons;
    [SerializeField] private IShopButton[] buttons;

    [SerializeField] private int level;

    [SerializeField] public GameObject[] firtsSlotWeapons;
    [SerializeField] public GameObject[] secondSlotWeapons;
    [SerializeField] public GameObject[] thirdSlotWeapons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerAttackController playerAtk = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttackController>();

       foreach (IShopButton button in buttons)
       {
            button.PlayerReference = playerAtk;
            button.ButtonManager = this;
            button.Level = level;
       }

        buttons[0].WeaponInStock = firtsSlotWeapons;
        buttons[1].WeaponInStock = secondSlotWeapons;
        buttons[2].WeaponInStock = thirdSlotWeapons;
    }

    public void UpdateButtons(IShopButton button)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (button == buttons[i])
            {
                buttons[i].DeactivateButton();
                return;
            }

            buttons[i].ActivateButton();
        }
    }
}
