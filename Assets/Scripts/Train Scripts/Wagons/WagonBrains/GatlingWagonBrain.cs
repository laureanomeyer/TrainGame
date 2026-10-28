using UnityEngine;

public class GatlingWagonBrain : WagonBrain
{
    [SerializeField] private WagonTurret turret;

    public WagonTurret Turret => turret;

    private void Awake()
    {
        WagonType = WagonType.ActiveTorret;


        if (turret == null)
            turret = GetComponentInChildren<WagonTurret>(true);
    }

    public override void Break()
    {
        base.Break();


        if (turret != null)
            turret.SetDisabled(true);
    }
}