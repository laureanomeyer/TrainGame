using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Dead")]
public class EnemyMovementDead : EnemyMovementSO
{
    [SerializeField] private bool useAcceleration = true;
    [SerializeField] private float acceleration = 50f;

    private float trainSpeed;
    private float currentSpeed;

    public override void Knockback(Enemy enemy) { }

    public override void Begin(Enemy enemy)
    {
        trainSpeed = RunManager.Instance.TrainSpeed;

        currentSpeed = useAcceleration ? 0f : trainSpeed;
    }

    protected override void MoveBehaviour(Enemy enemy)
    {
        if (useAcceleration)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                trainSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }

        Vector3 nextPos = enemy.rb.position +
            Vector3.left * currentSpeed * Time.fixedDeltaTime;

        enemy.rb.MovePosition(nextPos);
    }
}