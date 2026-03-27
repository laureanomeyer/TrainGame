using UnityEngine;

//capaz sirve mas que sea un SO..... ver a futuro

public class EnemyWeapon : MonoBehaviour, IEnemyWeapon
{

    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletDamage;
    [SerializeField] private float bulletDistance;
    [SerializeField] private float chanceToHit;

    [SerializeField] private GameObject bulletType;
    [SerializeField] private Transform bulletSpawn;

    public void Execute(GameObject target)
    {
        Instantiate(bulletType, bulletSpawn.position, Quaternion.identity);
    }
}
