using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Dead")]
public class EnemyMovementDead : EnemyMovementSO
{
    private float trainSpeed = 150;
    public override void Knockback(Enemy enemy) { }

    public override void Begin(Enemy enemy)
    {
        trainSpeed = RunManager.Instance.TrainSpeed;
    }

    protected override void MoveBehaviour(Enemy enemy)
    {
        Vector3 pos = enemy.rb.position;

        Vector3 nextPos = pos +
            Vector3.left * trainSpeed * Time.fixedDeltaTime;

        enemy.rb.MovePosition(nextPos);
    }
}