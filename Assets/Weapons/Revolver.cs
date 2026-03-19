using System.Net.Sockets;
using UnityEngine;

public class Revolver : MonoBehaviour, IWeapons
{
    public string id => "revolver";

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefap;

    [Header("BulletType")]
    [SerializeField] private BulletTypeScriptable bulletScriptable;

    //private BulletFactory bulletFactory;
    private BulletPool bulletPool;

    public void Shoot(Transform spawnPoint)
    {
        /*
        if (bulletFactory == null)
        {
            bulletFactory = GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletFactory>();
        }
        */

        if (bulletPool == null)
        {
            bulletPool = GameObject.FindGameObjectWithTag("Factory").GetComponent<BulletPool>();
        }

        //GameObject bulletInstance = bulletFactory.Create(bulletPrefap.GetComponent<IBullet>().id);
        //bulletInstance.GetComponent<BulletScript>().Initialize(bulletScriptable);

        bulletPool.ShootObject(transform.position, spawnPoint.rotation, bulletScriptable);
    }
}
