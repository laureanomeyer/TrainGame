using UnityEngine;

[CreateAssetMenu(fileName = "WeaponWithLegadoInStock", menuName = "Store/Weapons Stock/WeaponLegado")]
public class WeaponWithLegadoInStockSO : WeaponInStocSO
{
    [Header("Legado")]
    public string legadoUnlockDescription;
    public string legadoDescription;
}
