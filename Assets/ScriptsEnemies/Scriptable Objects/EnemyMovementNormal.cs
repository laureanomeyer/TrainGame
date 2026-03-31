using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Normal")]
public class EnemyMovementNormal : EnemyMovementSO
{
    public float desiredDistance = 5f;
    public float tolerance = 0.5f;
    public float limitZ;

    
    private Transform train;

    public override void Move(Enemy enemy)
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

        if (pos.z < limitZ)
            pos.z = limitZ;
        

        if(pos.y < 5)
           pos.y = 5; 

        enemy.transform.position = pos;
    }
}