using UnityEngine;

[CreateAssetMenu(fileName = "SpencerInStock", menuName = "Store/Weapons Stock/SpencerInStock")]
public class SpencerInStockSO : WeaponWithLegacyInStockSO
{
    public override bool CheckUnlockLegacy()
    {
        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        return playerData.unlockedLegado.UnlockedSpencer;
    }
}
