using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private bool isTail;
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;

    public Transform Tail => tail;
    public Transform Head => head;

    public bool IsTail => isTail;   

    public float Width => Vector3.Distance(head.position, tail.position);   

    public void setUp(Transform followTransform)
    {
        head = followTransform;
        isTail = false;
    }
    public void SetTail()
    {
        isTail = true;
        head = null;
    }
    public void Move()
    {
        if (!isTail)
        {
            transform.position = head.position;
        }
    }

    public void PlaceHeadAt(Vector3 worldPosition)
    {
        Vector3 offset = transform.position - head.position;
        transform.position = worldPosition + offset;
    }

    public void PlaceAfter(MapTile otherTile)
    {
        PlaceHeadAt(otherTile.Tail.position);
    }
    public bool IsPastPoint(float recycleX)
    {
        Debug.Log("Hola");
        return head.position.x < recycleX;
    }

 
}
