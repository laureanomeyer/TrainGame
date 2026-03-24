using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private bool isMapHead;
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;
    private float offset;
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
        offset = Vector3.Distance(transform.position, tail.position);          
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
            Vector3 adjustedTarget = new Vector3 (target.transform.position.x - offset, target.transform.position.y, target.transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, adjustedTarget, speed * Time.deltaTime);
        }
        
    }
}
