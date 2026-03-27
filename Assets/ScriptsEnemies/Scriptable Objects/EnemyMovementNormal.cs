using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Normal")]


public class EnemyMovementNormal : EnemyMovementSO
{
    public float desiredDistance = 5f;
    public float tolerance = 0.5f;

    public override void Move(Enemy enemy)
    {
        GameObject train = enemy.Target; //Temporal. Cambiar cuando se mergee con el train de lau

        if (train == null) return;

        Vector3 dir = train.transform.position - enemy.transform.position;
        float distance = dir.magnitude;

        dir.Normalize();

        //  Muy lejos -> acercarse
        if (distance > desiredDistance + tolerance)
        {
            enemy.transform.position += dir * enemy.Speed * Time.deltaTime;
        }
        //  Muy cerca -> alejarse
        else if (distance < desiredDistance - tolerance)
        {
            enemy.transform.position -= dir * enemy.Speed * Time.deltaTime;
        }
    }
}
