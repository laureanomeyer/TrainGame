using UnityEngine;

[CreateAssetMenu(fileName = "WagonInStock", menuName = "Store/Wagon")]
public class WagonInStockSO : ScriptableObject
{
    [Header("Wagon")]
    [SerializeField] private GameObject wagon;
    public GameObject Wagon => wagon;

    [SerializeField] public string wagonName;

    [Header("Price")]
    [SerializeField] private float price;
    public float Price => price;

    [Header("Description")]
    [SerializeField] private string description;
    public string Description => description;

    [Header("Shop Model")]
    [SerializeField] public GameObject shopModel;

    [Header("Upgrade")]
    [Tooltip("Dejar vacío si este vagón no puede mejorarse (regla de 'Costo-Recompensa').")]
    [SerializeField] private WagonLevelSetSO levelSet;
    public WagonLevelSetSO LevelSet => levelSet;

}
