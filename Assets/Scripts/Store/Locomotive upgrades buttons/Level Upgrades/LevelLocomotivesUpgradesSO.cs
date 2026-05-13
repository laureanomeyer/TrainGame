using UnityEngine;

[CreateAssetMenu(fileName = "Level Upgrades", menuName = "Store/Level locomotives upgrades")]

public class LevelLocomotivesUpgradesSO : ScriptableObject
{
    [Header("Level")]
    [SerializeField] public int level;

    [Header("Upgrades")]
    [SerializeField] public float maxHp;
    [SerializeField] public float defense;
    [SerializeField] public float goldMultiplier;
    [SerializeField] public float damageMultiplier;
    [SerializeField] public float attackSpeed;
}
