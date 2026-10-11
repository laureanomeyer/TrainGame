using UnityEngine;

public class MotivationalBrain : WagonBrain, IWeaponBuffer
{
    [SerializeField] private MotivationalSideTurret[] turrets;

    [Header("Stats de las torretas")]
    [SerializeField, Min(0f)] private float damage = 1f;
    [Tooltip("Tiempo en segundos entre disparo y disparo")]
    private float baseDamage;
    private float damageBuff = 0;

    [SerializeField, Min(0.01f)] private float rofTime = 1.5f;
    [Tooltip("Cantidad de disparos (cartuchos) por cargador")]
    private float baseRoFTime;
    private float rofBuff = 0;
    [SerializeField, Min(1)] private int ammunition = 6;
    [Tooltip("Tiempo en segundos que tarda en recargar")]
    private float baseAmmunition;
    [SerializeField, Min(0f)] private float reloadTime = 2.5f;
    private float baseReloadTime;

    private void Awake()
    {
        WagonType = WagonType.PassiveTorret;

        baseDamage = damage;
        baseRoFTime = rofTime;
        baseAmmunition = ammunition;
        baseReloadTime = reloadTime;

        foreach (var turret in turrets)
        {
            turret.bulletPool = ServiceLocator.Get<BulletPool>();
            turret.Initialize(damage, rofTime, ammunition, reloadTime);
        }
    }

    public override void Break()
    {
        base.Break();

        foreach (var turret in turrets)
        {
            turret.isAlive = false;
        }
    }

    public void BufferRoF(float buffer)
    {
        rofBuff += buffer;
        rofTime = baseRoFTime * (1f - rofBuff / 100f);
    }

    public void DebuffRoF(float buffer)
    {
        rofBuff -= buffer;
        if (rofBuff > 0)
        {
            rofTime = baseRoFTime * (1f - rofBuff / 100f);
        }
        else
        {
            rofTime = baseRoFTime;
        }
    }

    public void BufferDamage(float buffer)
    {
        damageBuff += buffer;
        damage = baseDamage * (1f + damageBuff / 100f);
    }

    public void DebuffDamage(float buffer)
    {
        damageBuff -= buffer;
        if (damageBuff > 0)
        {
            damage = baseDamage * (1f + damageBuff / 100f);
        }
        else
        {
            damage = baseDamage;
        }
    }
}