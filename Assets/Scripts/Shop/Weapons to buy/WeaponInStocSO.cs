using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInStock", menuName = "Store/Weapon")]
public class WeaponInStocSO : ScriptableObject
{
    [Header("Weapon")]
    [SerializeField] private GameObject weapon;
    public GameObject Weapon => weapon;

    [Header("Price")]
    [SerializeField] private float price;
    public float Price => price;
}
