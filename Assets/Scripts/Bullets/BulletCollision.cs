using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    private BulletScript bulletScript;

    private void Start()
    {
        bulletScript = GetComponent<BulletScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("deadWall"))
        {
            bulletScript.Deactivate();
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            bulletScript.Deactivate();
        }
    }
}
