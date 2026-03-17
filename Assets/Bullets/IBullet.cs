using UnityEngine;

public interface IBullet 
{
    public string id { get; }

    public void ResetState();

    public void Movement();

    void DestroyBullet();
}
