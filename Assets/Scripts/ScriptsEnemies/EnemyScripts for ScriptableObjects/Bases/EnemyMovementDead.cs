using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Dead")]
public class EnemyMovementDead : EnemyMovementSO
{
    [SerializeField] private bool useAcceleration = true;
    [SerializeField] private float acceleration = 50f;

    private float trainSpeed;

    public override void Knockback(Enemy enemy) { }

    public override void Begin(Enemy enemy)
    {
        trainSpeed = RunManager.Instance.TrainSpeed;

        enemy.CurrentMovementSpeed = useAcceleration ? 0f : trainSpeed;
    }

    protected override void MoveBehaviour(Enemy enemy)
    {
        if (useAcceleration)
        {
            enemy.CurrentMovementSpeed = Mathf.MoveTowards(
                enemy.CurrentMovementSpeed,
                trainSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }

        Vector3 nextPos = enemy.rb.position +
            Vector3.left * enemy.CurrentMovementSpeed * Time.fixedDeltaTime;

        enemy.rb.MovePosition(nextPos);
    }
}