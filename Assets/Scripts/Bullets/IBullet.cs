using UnityEngine;
using UnityEngine.Pool;

public interface IBullet 
{
    //Nombre de la bala
    string id { get; }

    public float Damage { get; set; }

    public int Speed { get; set; }

    public bool DestroyOnEnemy { get; }

    //Referencia para utlizar la pool de unity 
    IObjectPool<GameObject> BulletPool { set; }

    //Resetea el estado de la bala para lanzarla de nuevo
    public void ResetState(BulletTypeScriptable type);

    //Movimiento de la bala
    public void Movement();

    //Desactiva la bala para devolverla a la pool
    public void Deactivate();
}
