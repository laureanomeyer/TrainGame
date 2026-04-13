using UnityEngine;

[CreateAssetMenu(fileName = "WagonInStock", menuName = "Store/Wagon")]
public class WagonInStockSO : ScriptableObject
{
    [Header("Wagon")]
    [SerializeField] private GameObject wagon;
    public GameObject Wagon => wagon;

    [Header("Price")]
    [SerializeField] private float price;
    public float Price => price;

    [Header("Description")]
    [SerializeField] private string description;
    public string Description => description;
}
