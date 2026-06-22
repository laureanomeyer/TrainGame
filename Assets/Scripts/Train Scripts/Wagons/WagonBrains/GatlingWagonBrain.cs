using UnityEngine;

public class GatlingWagonBrain : WagonBrain
{
    [SerializeField] private WagonTurret turret;

    public WagonTurret Turret => turret;
}