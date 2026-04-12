using UnityEngine;

public enum WagonTypes { coal, gold }

[CreateAssetMenu(menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    public float health;
    public float range;
    public float speed;
    public float distance;
    public float attackCooldown;
    public WagonTypes target;

    public float gold;

    public EnemyAttackSO attack;     // logica ataque
    public EnemyBrainSO brain;       // logica targeteo
    public EnemyMovementSO movement; //logica movimiento
}
