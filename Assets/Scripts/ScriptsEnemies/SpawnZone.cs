using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [SerializeField] private Vector2 size;

    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float followSpeed = 5f;

    public Vector3 GetRandomPoint(float positiveLimit, float negativeLimit)
    {
        Vector3 center = transform.position;

        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(center.x - size.x / 2, center.x + size.x / 2), 0, Random.Range(center.z - size.y / 2, center.z + size.y / 2));

            // SOLO fuera del rango del tren
            bool outsideTrainRange = randomPos.z > positiveLimit || randomPos.z < negativeLimit;

            if (outsideTrainRange)
            {
                return randomPos;
            }
        }

        return center;
    }

    private void LateUpdate()
    {
        if (cameraTarget == null)
            return;

        Vector3 targetPos = cameraTarget.position;

        targetPos.y = transform.position.y;

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.y));
    }
}