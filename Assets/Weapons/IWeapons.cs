using UnityEngine;

public interface IWeapons
{
    public string id { get; }

    public void Shoot(Transform spawnPoint);
}
