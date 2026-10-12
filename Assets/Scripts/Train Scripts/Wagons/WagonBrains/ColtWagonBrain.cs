using System.Collections.Generic;
using UnityEngine;

public class ColtWagonBrain : WagonBrain, IWagonPoolData
{
    [SerializeField] private WagonFixedTurret[] turrets;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxCapacity = 20;
    public int DefaultCapacity { get => defaultCapacity; set => defaultCapacity = value; }
    public int MaxCapacity { get => maxCapacity; set => maxCapacity = value; }

    private void Awake()
    {
        WagonType = WagonType.PassiveTorret;

        foreach (var turret in turrets)
        {
            turret.bulletPool = ServiceLocator.Get<BulletPool>();
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
}