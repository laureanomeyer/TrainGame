using UnityEngine;

public class Revolver : MonoBehaviour, IWeapons
{
    public string id => "revolver";

    [Header("Bullet")]
    [SerializeField] GameObject bulletPrefap;

    public void Shoot(Transform spawnPoint)
    {
        Instantiate(bulletPrefap, spawnPoint.position, spawnPoint.rotation);
    }
}
