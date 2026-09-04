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

    public void Execute(Transform target, float damage)
    {
        Shoot(target, damage);
    }

    public void Shoot(Transform target, float damage)
    {
        if (target == null) return;

        Vector3 dir = (target.transform.position - bulletSpawn.position).normalized;

        //GameObject bulletGO = ObjectPoolManager.SpawnObject(bulletType, bulletSpawn.position, Quaternion.LookRotation(dir));
        GameObject bulletGO = Instantiate(bulletType, bulletSpawn.position, Quaternion.LookRotation(dir));

        EnemyBullet bullet = bulletGO.GetComponent<EnemyBullet>();
        bullet.Init(dir, damage);
        
    }

}
