using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField] private bool isMapHead;
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;
    [SerializeField] private MeshRenderer mesh;
    private float offset;
    private Vector3 offsetVect;
    public Transform Tail => tail;
    public Transform Head => head;

    public bool IsMapHead => isMapHead;   

    public float Offset => offset;
    public Vector3 OffsetVect => offsetVect;

    private void Awake()
    {
        offset = Vector3.Distance(transform.position, tail.position);
        offsetVect = new Vector3(offset, 0, 0);
        //Debug.Log($"offset: {offset} | tail local: {tail.localPosition} | tail world: {tail.position}");
    }
    public void SetUpWithMesh(Transform followTransform, MeshRenderer mesh)
    {
        head = followTransform;
        isMapHead = false;

        if(mesh != null)
            this.mesh = mesh;
    }
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
            transform.position = head.position + offsetVect;
        }
    }
    public void MoveHead(Vector3 target, float speed)
    {
        if (isMapHead)
        {
            Vector3 adjustedTarget = new Vector3(
                target.x - offset * 5,
                target.y,
                target.z
            );
            transform.position = Vector3.MoveTowards(transform.position, adjustedTarget, speed * Time.deltaTime);
        }
    }
}
