using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Normal")]
public class EnemyMovementNormal : EnemyMovementSO
{
    public float desiredDistance = 5f;
    public float tolerance = 0.5f;


    private Transform train;

    public override float SetLimitZ()
    {
        return Random.Range(-5, -1.5f);
    }

    public override void Move(Enemy enemy, float limitZ)
    {
        train = enemy.Target;

        if (train == null)
            return;

        Vector3 pos = enemy.transform.position;

        Vector3 dir = train.position - pos;
        float distance = dir.magnitude;

        dir.Normalize();

        // Muy lejos -> acercarse
        if (distance > desiredDistance + tolerance)
        {
            pos += dir * enemy.Speed * Time.deltaTime;
        }
        // Muy cerca -> alejarse
        else if (distance < desiredDistance - tolerance)
        {
            pos -= dir * enemy.Speed * Time.deltaTime;
        }

        float stopZ = limitZ + enemy.Range;


        if (pos.z < stopZ)
        {
            pos.z = stopZ;
        }


        if (pos.y < 0)
           pos.y = 0; 

        enemy.transform.position = pos;
    }
}