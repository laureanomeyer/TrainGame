using UnityEngine;

[CreateAssetMenu(fileName = "ColtInStock", menuName = "Store/Weapons Stock/ColtInStock")]
public class ColtInStock : WeaponWithLegacyInStockSO
{
    public override bool CheckUnlockLegacy()
    {
        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        return playerData.unlockedLegado.UnlockedColt;
    }
}
