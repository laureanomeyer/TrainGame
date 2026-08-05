using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon data", menuName = "Weapons/Weapon data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Type of weapon")]
    [SerializeField] public WeaponType type;

    [Header("Type of shoot")]
    [SerializeField] public TypeOfShoot typeOfShootSO;

    [Header("Damage")]
    [SerializeField] public float damage;

    [Header("Ammun")]
    [SerializeField] public int ammun;

    [Header("Reload time")]
    [SerializeField] public float reloadTime;

    [Header("Rate of fire")]
    [SerializeField] public float rateOfFire;

}
