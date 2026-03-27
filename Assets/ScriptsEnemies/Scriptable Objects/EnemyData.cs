using UnityEngine;

public enum WagonTypes { coal, gold }

[CreateAssetMenu(menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    public float range;
    public float speed;
    public float distance;
    public WagonTypes target;

    public EnemyAttackSO attack;     // logica ataque
    public EnemyBrainSO brain;       // logica targeteo
    public EnemyMovementSO movement; //logica movimiento
}
