using System.Net.Sockets;
using UnityEngine;

public class Revolver : MonoBehaviour, IWeapons
{
    public string id => "revolver";

    //Tipos de balas que utiliza el arma
    [Header("BulletType")]
    [SerializeField] private BulletTypeScriptable bulletScriptable;

    //Referencia a la pool de balas
    private BulletPool bulletPool;

    public void Shoot(Transform spawnPoint)
    {
        bulletPool.ShootObject(spawnPoint.position, spawnPoint.rotation, bulletScriptable);
    }

    public void SetPool(BulletPool pool)
    {
        bulletPool = pool;
    }
}
