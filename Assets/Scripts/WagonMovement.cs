using System.IO;
using UnityEngine;

public class WagonMovement : MonoBehaviour, IWagon
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;
    public Transform Tail => tail;

    private Transform targetTail;
    [SerializeField] private float speed;
    public void Initialize(Transform target)
    {
        this.targetTail = target;
    }
    
    void LateUpdate()
    {
        Move();
    }

    void Move()
    {
        if (targetTail == null) return;   
        transform.position = targetTail.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTail.rotation, 0.07f);
        tail.rotation = transform.rotation;

    }
    void Interact()
    {

    }
}
