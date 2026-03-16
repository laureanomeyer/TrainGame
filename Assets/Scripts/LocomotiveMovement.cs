using UnityEngine;

public class LocomotiveMovement : MonoBehaviour, IWagon
{
    [SerializeField] private float speed;
    [SerializeField] private Transform[] targets;
    [SerializeField] private Transform nextTarget;
    [SerializeField] private int nextStopIndex = 0;

    void Start()
    {
        nextTarget = targets[0];
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (Vector3.Distance(transform.position, nextTarget.position) < 0.01f)
        { 
            nextStopIndex++;

            if (nextStopIndex >= targets.Length) 
            {
                nextStopIndex = 0;
            }

            
            nextTarget = targets[nextStopIndex];
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, nextTarget.position, speed * Time.deltaTime);
            Vector3 direction = nextTarget.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180 * Time.deltaTime);

        }
    }

    void Interact()
    {

    }
}
