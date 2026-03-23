using UnityEngine;

public class MapTile : MonoBehaviour
{
    private bool isTail;
    private Transform head;

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

}
