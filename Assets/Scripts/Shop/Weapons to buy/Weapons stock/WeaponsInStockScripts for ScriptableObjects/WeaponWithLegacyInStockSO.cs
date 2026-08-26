using UnityEngine;

[CreateAssetMenu(fileName = "WeaponWithLegadoInStock", menuName = "Store/Weapons Stock/WeaponLegado")]
public class WeaponWithLegacyInStockSO : WeaponInStocSO
{
    [Header("Legado")]
    public string legacyUnlockDescription;
    public string legacyDescription;

    public virtual bool CheckUnlockLegacy()
    {
        Debug.Log("Revisar legado");
        return false;
    }
}
