using UnityEngine;

[CreateAssetMenu(fileName = "Collection of wagons", menuName = "Store/Wagon collections")]

public class ShopWagonCollectionSO : ScriptableObject
{
    [Header("Level")]
    [SerializeField] public int level;

    [Header("Wagon in collection")]
    [SerializeField] private WagonInStockSO[] wagonCollection;
    public WagonInStockSO[] WagonCollection => wagonCollection;
}
