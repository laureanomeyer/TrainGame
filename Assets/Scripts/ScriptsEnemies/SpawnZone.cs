using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    public Vector3 size;
    bool registered;

    public void Register()
    {
        if (registered) return;

        RunManager.Instance.SpawnManager.RegisterZone(this);

        registered = true;
    }

    public Vector3 GetRandomPoint()
    {
        Vector3 c = transform.position;

        return new Vector3(Random.Range(c.x - size.x / 2, c.x + size.x / 2), c.y, Random.Range(c.z - size.z / 2, c.z + size.z / 2));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
