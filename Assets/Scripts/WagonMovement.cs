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
        transform.position = head.position;
        transform.rotation = head.rotation;
    }
    void Interact()
    {

    }
}
