using System.IO;
using UnityEngine;

public class WagonMovement : MonoBehaviour, IWagon
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;
    public Transform Tail => tail;

    [SerializeField] private float speed;
    public void Initialize(Transform head)
    {
        this.head = head;
    }
    
    void LateUpdate()
    {
        Move();
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position,head.position, speed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, head.rotation, speed * 10 * Time.deltaTime);
    }
    void Interact()
    {

    }
}
