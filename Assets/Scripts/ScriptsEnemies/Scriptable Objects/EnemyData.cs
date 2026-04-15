using UnityEngine;

public enum WagonTypes { coal, gold }
public enum RangeType { close, distance , medium }

[CreateAssetMenu(menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    public float health;
    public float range;
    public float speed;
    public float damage;
    public float attackCooldown;
    
    //public WagonTypes target;

    public RangeType rangeType;

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
            case RangeType.close:
                range = 5f;
                break;

            case RangeType.medium:
                range = 20f;
                break;

            case RangeType.distance:
                range = 30f;
                break;
        }
    }

}
