using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Dead")]
public class EnemyMovementDead : EnemyMovementSO
{
    public override void Move(Enemy enemy)
    {

        Vector3 pos = enemy.rb.position;

        Vector3 nextPos = pos +
            Vector3.left * speed * Time.fixedDeltaTime;

        enemy.rb.MovePosition(nextPos);
    }

    public override void Knockback(Enemy enemy) { }

    public override void Begin(Enemy enemy)
    {
    }
}