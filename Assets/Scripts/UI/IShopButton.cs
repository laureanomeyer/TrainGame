using NUnit.Framework;
using UnityEngine;

public interface IShopButton
{
    public PlayerAttackController PlayerReference { get; set; }
    public WeaponShopButtonManager ButtonManager { get; set; }

    //public bool IsButtonActiv { get; set; }

    public int Level { get; set; }


    public GameObject[] WeaponInStock { get; set; }

    public void ActivateButton();

    public void DeactivateButton();

    public void SetValuesInStock();
}
