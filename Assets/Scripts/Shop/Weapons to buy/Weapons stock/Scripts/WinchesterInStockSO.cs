using UnityEngine;

[CreateAssetMenu(fileName = "WinchesterInStock", menuName = "Store/Weapons Stock/WinchesterInStock")]
public class WinchesterInStockSO : WeaponWithLegacyInStockSO
{
    public override bool CheckUnlockLegacy()
    {
        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        return playerData.unlockedLegado.UnlockedWinchester;
    }
}
