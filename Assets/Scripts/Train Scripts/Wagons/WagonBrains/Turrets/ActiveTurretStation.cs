using UnityEngine;

public class ActiveTurretStation : MonoBehaviour
{
    [SerializeField] private WagonActiveTurret turret;

    public WagonActiveTurret Turret => turret;
}