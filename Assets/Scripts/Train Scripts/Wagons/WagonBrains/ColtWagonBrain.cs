using System.Collections.Generic;
using UnityEngine;

public class ColtWagonBrain : WagonBrain
{
    [SerializeField] private WagonFixedTurret[] turrets;

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