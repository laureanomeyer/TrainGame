using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Losing")]
public class EnemyMovementLosing : EnemyMovementSO
{
    [Header("Losing settings")]
    [SerializeField] float timeBeforeLosing = 5f;
    [SerializeField] float loseSpeed = 3f;

    public override void Move(Enemy enemy)
    {
        Vector3 train = enemy.TargetWagon.Middle;
        if (train == null) return;

        Vector3 pos = enemy.rb.position;

        // TimeAlive solo crece, así que una vez isLosing = true, queda así
        // para siempre durante la vida de este enemigo (irreversible).
        bool isLosing = enemy.TimeAlive >= timeBeforeLosing;

        // =========================
        // PERDIENDO: SOLO retrocede en Z, X queda congelado, no importa el carril
        // =========================
        if (isLosing)
        {
            Vector3 losingPos = new Vector3(pos.x  - loseSpeed * Time.deltaTime, pos.y, pos.z);
            enemy.rb.MovePosition(losingPos);
            return;
        }

        // =========================
        // COMPORTAMIENTO NORMAL (mientras no esta perdiendo)
        // =========================
        float minZ = enemy.Limits.Item1;
        float maxZ = enemy.Limits.Item2;
        bool insideLane = pos.z <= minZ && pos.z >= maxZ;

        if (!insideLane)
        {
            Vector3 dir = (train - pos).normalized;
            enemy.rb.MovePosition(pos + dir * enemy.Speed * Time.deltaTime);
            return;
        }
        else
        {
            float targetX = train.x;
            float distanceToX = Mathf.Abs(pos.x - targetX);
            float stopDistance = 5f;

            if (distanceToX <= stopDistance) return;

            Vector3 lateralDir = pos.x < targetX ? Vector3.right : Vector3.left;
            Vector3 nextPos = pos + lateralDir * enemy.Speed * Time.deltaTime;
            enemy.rb.MovePosition(nextPos);
            
        }

    }

    public override void Knockback(Enemy enemy)
    {
        enemy.rb.AddForce(enemy.rb.transform.forward * 10, ForceMode.Impulse);
    }
}