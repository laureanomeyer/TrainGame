using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private bool isMapHead;
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;

    public Transform Tail => tail;
    public Transform Head => head;

    public bool IsMapHead => isMapHead;   

    public float Width => Vector3.Distance(head.position, tail.position);   

    public void SetUp(Transform followTransform)
    {
        head = followTransform;
        isMapHead = false;
    }
    public void SetTail()
    {
        isMapHead = true;
        head = null;
    }
    public void Move()
    {
        if (!isMapHead)
        {
            transform.position = head.position;
        }
    }
    public void MoveHead(Transform target, float speed)
    {
        if (isMapHead) 
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
        
    }
}
