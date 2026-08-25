using UnityEngine;

[CreateAssetMenu(fileName = "CoachInStock", menuName = "Store/Weapons Stock/CoachInStock")]
public class CoachInStockSO : WeaponWithLegacyInStockSO
{
    public override bool CheckUnlockLegacy()
    {
        PlayerData playerData = ServiceLocator.Get<PlayerData>();

        return playerData.unlockedLegado.UnlockedCoach;
    }
}
