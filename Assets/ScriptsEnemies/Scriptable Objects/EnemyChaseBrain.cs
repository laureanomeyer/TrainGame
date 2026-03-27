using UnityEngine;
[CreateAssetMenu(menuName = "Enemy/Brain/Chase")]

public class EnemyChaseBrain : EnemyBrainSO
{
    public override void Begin(Enemy enemy)
    {
        Debug.Log("sinapsis");
    }

    public override void Tick(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }
}
