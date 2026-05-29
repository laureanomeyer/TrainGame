using UnityEngine;
[CreateAssetMenu(fileName = "LocomotiveStats", menuName = "Scriptable Objects/LocomotiveStats")]
public class LocomotiveStatsSO : ScriptableObject
{
    public float maxHp;
    public float defense;
    public float goldMultyplier;
    public float damageMultyplier;
    public float attackSpeed;

    public float fuelOptimizer;
    public float baseSpeed;
}
