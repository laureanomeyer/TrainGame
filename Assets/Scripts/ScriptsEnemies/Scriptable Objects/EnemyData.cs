using System.Collections.Generic;
using UnityEngine;

public enum TargetType { Random, Passives, Locomotive, Gold, Actives }
public enum RangeType
{
    Close = 15,
    Medium = 30,
    Long = 40
}
public enum AttackType { Shoot, Explosion, None }

[CreateAssetMenu(menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    public float health;
    public float speed;
    public float damage;
    public float attackCooldown;
    public float range;

    public RangeType rangeType;
    public TargetType[] target;

    public float gold;

    public SkinnedMeshRenderer enemyMesh;
    public SkinnedMeshRenderer horseMesh;

    public Material[] material;

    public EnemyAttackSO attack;     // logica ataque
    public EnemyBrainSO brain;       // logica targeteo
    public EnemyMovementSO movement; //logica movimiento

    public EnemySkillSO skill;

    private void OnValidate()
    {
        ApplyRangePreset();
    }

    private void ApplyRangePreset()
    {
        switch (rangeType)
        {
            case RangeType.Close:
                range = (float)RangeType.Close;
                break;

            case RangeType.Medium:
                range = (float)RangeType.Medium;
                break;

            case RangeType.Long:
                range = (float)RangeType.Long;
                break;
        }
    }
}