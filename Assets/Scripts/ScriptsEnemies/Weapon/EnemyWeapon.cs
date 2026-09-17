using UnityEngine;

//capaz sirve mas que sea un SO..... ver a futuro

public class EnemyWeapon : MonoBehaviour, IEnemyWeapon
{
    [SerializeField] private GameObject bulletType;
    Transform bulletSpawn;


    private void Start()
    {
        bulletSpawn = GetComponentInChildren<Transform>();
    }

    public void Execute(IWagon target, float damage)
    {
        Shoot(target, damage);
    }

    public void Shoot(IWagon target, float damage)
    {
        if (target == null || target.Head == null || target.Tail == null) return;

        Vector3 targetPosition = (target.Head.position + target.Tail.position) * 0.5f;
        Vector3 dir = (targetPosition - bulletSpawn.position).normalized;

        //GameObject bulletGO = ObjectPoolManager.SpawnObject(bulletType, bulletSpawn.position, Quaternion.LookRotation(dir));
        GameObject bulletGO = Instantiate(bulletType, bulletSpawn.position, Quaternion.LookRotation(dir));

        EnemyBullet bullet = bulletGO.GetComponent<EnemyBullet>();
        bullet.Init(dir, damage);
    }

}
