using UnityEngine;
using UnityEngine.Pool;

public interface IBullet 
{
    string id { get; }

    IObjectPool<GameObject> BulletPool { set; }

    public void ResetState(BulletTypeScriptable type);

    public void Movement();

    public void Deactivate();

    public void Initialize(BulletTypeScriptable type);
}
