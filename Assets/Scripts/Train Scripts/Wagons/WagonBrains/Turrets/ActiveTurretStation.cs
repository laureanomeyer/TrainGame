using UnityEngine;

public class ActiveTurretStation : MonoBehaviour
{
    [SerializeField] private GatlingWagonBrain turret;

    public GatlingWagonBrain Turret => turret;
}