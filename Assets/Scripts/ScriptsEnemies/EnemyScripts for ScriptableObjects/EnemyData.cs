using System.Collections.Generic;
using UnityEngine;

public enum WagonType { Random, Passive, PassiveTorret, Active, ActiveTorret, Locomotive, Gold }
public enum RangeType
{
    Close = 15,
    Medium = 25,
    Long = 35
}

public enum DropType {Gold, Coal}

[CreateAssetMenu(menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    public float health;
    public float speed;
    public float damage;
    public float attackCooldown;

    public RangeType rangeType;
    public WagonType targetPreference;
    public DropType drop;
    public float dropAmount;

    public SkinnedMeshRenderer enemyMesh;
    public SkinnedMeshRenderer horseMesh;

    public Material[] material;

    

    public EnemyAttackSO attack;     // logica ataque
    public EnemyBrainSO brain;       // logica targeteo
    public EnemyMovementSO movement; //logica movimiento

    public EnemySkillSO skill;

    public EnemyAnimationSO animation;

    
}