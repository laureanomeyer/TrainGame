using UnityEngine;
[CreateAssetMenu(menuName = "Enemy/Brain/Target")]

public class EnemyBrainTarget : EnemyBrainSO
{
    public override void Begin(Enemy enemy)
    {

    }

    public override Transform SetTarget(Enemy enemy)
    {
        return enemy.TargetList[Random.Range(0 , enemy.TargetList.Count)].Transform;
    }

    public override void Tick(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }
}
