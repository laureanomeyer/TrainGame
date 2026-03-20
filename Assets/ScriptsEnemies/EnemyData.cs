using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{

    //public EnemyMovementSO movement;
    public EnemyAttackSO attack;
    public EnemyBrainSO brain;
}