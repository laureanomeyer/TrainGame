using NUnit.Framework;
using UnityEngine;

public interface IShopButton
{
    public PlayerBrain PlayerReference { get; set; }
    public WeaponShopButtonManager ButtonManager { get; set; }

    public int Level { get; set; }

    public WeaponInStocSO[] WeaponInStock { get; set; }

    public void ActivateButton();

    public void DeactivateButton();

    public void SetValues(int level);
}
