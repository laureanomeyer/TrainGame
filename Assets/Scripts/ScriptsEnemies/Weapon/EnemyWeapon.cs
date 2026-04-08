using UnityEngine;

//capaz sirve mas que sea un SO..... ver a futuro

public class EnemyWeapon : MonoBehaviour, IEnemyWeapon
{

    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletDamage;
    [SerializeField] private float bulletDistance;
    [SerializeField] private float chanceToHit;

    [SerializeField] private GameObject bulletType;
    Transform bulletSpawn;

    private void Start()
    {
        bulletSpawn = GetComponentInChildren<Transform>();
    }

    public void Execute(Transform target)
    {
        Shoot(target);
    }

    public void Shoot(Transform target)
    {
        if (target == null) return;

        Vector3 dir = (target.transform.position - bulletSpawn.position).normalized;

        GameObject bulletGO = Instantiate(
            bulletType,
            bulletSpawn.position,
            Quaternion.LookRotation(dir)
        );

        EnemyBullet bullet = bulletGO.GetComponent<EnemyBullet>();
        bullet.Init(dir);

    }

}
