using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon data", menuName = "Weapons/Weapon data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Type of weapon")]
    [SerializeField] public WeaponType type;

    [Header("Damage")]
    [SerializeField] public float damage;

    [Header("Rate of fire")]
    [SerializeField] public float rateOfFire;

    [Header("Shot Strenght")]
    [SerializeField] public float shotStrenght = 1;

    [Header("Reload time")]
    [SerializeField] public float reloadTime;

    [Header("Ammun")]
    [SerializeField] public int ammun;

    [Header("Ammun")]
    [SerializeField] public Sprite weaponSprite;
}
