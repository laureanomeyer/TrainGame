using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WeaponInStock", menuName = "Store/Weapon")]
public class WeaponInStocSO : ScriptableObject
{
    [Header("Weapon")]
    [SerializeField] private GameObject weapon;
    public GameObject Weapon => weapon;

    [Header("ScriptableObject")]
    [SerializeField] private WeaponDataSO weaponData;
    public WeaponDataSO WeaponData => weaponData;

    [Header("Items")]
    [SerializeField] private float price;
    [SerializeField] private Sprite gunSprite;
    public float Price => price;
    public Sprite GunSprite => gunSprite;
}
