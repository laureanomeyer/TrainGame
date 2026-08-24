using UnityEngine;

[CreateAssetMenu(fileName = "WeaponCollection", menuName = "Store/Weapon Collection")]
public class WeaponCollectionInStockSO : ScriptableObject
{
    public int Level;
    
    public WeaponInStocSO[] weaponCollection;
}
