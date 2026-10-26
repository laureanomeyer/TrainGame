using System.Collections.Generic;
using UnityEngine;

public enum WagonType { Random, Passive, PassiveTorret, Active, ActiveTorret, Locomotive, Gold }
public enum RangeType
{
    Close = 15,
    Medium = 20,
    Long = 30
}

public enum EnemyVisualID 
{
    Carbonero,Recluta,Bandido,
    Sheriff,Juggernaut,SharpShooter,
    Blindador,Dinamitero,Stunner,
    Magnet,Pistolero,SandMan,
    KuChau,OreThief,Welder,
    Leader,Kamikaze,OilPourer
}

public enum DropType {Gold, Coal}

[CreateAssetMenu(menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    public float health;
    public float damage;
    public float attackCooldown;

    public RangeType rangeType;
    public WagonType targetPreference;
    public DropType drop;
    public float dropAmount;

    [Header("Legacy direct mesh binding (kept for compatibility)")]
    public SkinnedMeshRenderer enemyMesh;
    public SkinnedMeshRenderer horseMesh;

    [Header("New single-prefab visual approach")]
    public EnemyVisualID visualType;

    public Material[] material;

    public EnemyAttackSO attack;     // logica ataque
    public EnemyBrainSO brain;       // logica targeteo
    public EnemyMovementSO movement; //logica movimiento

    public EnemySkillSO skill;

    public EnemyAnimationSO animation;

}