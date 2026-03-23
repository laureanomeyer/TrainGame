using UnityEngine;

public class MapTile : MonoBehaviour
{
    private bool isTail;
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;

    public Transform Tail => tail;
    public Transform Head => head;

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

    public bool IsPastPoint(float recycleX)
    {
        Debug.Log("Hola");
        return head.position.x < recycleX;
    }

    public void PlaceAfter(MapTile otherTile)
    {
        Vector3 distanceOffLedge = transform.position - head.position;
        transform.position = otherTile.head.position + distanceOffLedge;
    }

}
