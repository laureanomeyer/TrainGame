using System.Collections.Generic;
using UnityEngine;

public enum TargetType { Random, Passives, Locomotive, Gold, Actives }
public enum RangeType { Close, Medium, Long }
public enum AttackType { Shoot, Explosion, None}

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

    public EnemyAttackSO attack;     // logica ataque
    public EnemyBrainSO brain;       // logica targeteo
    public EnemyMovementSO movement; //logica movimiento

    private void OnValidate()
    {
        ApplyRangePreset();
    }

    private void ApplyRangePreset()
    {
        switch (rangeType)
        {
            case RangeType.Close:
                range = 5f;
                break;

            case RangeType.Medium:
                range = 20f;
                break;

            case RangeType.Long:
                range = 30f;
                break;
        }
    }
}
 