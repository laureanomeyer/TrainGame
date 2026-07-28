using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [SerializeField] private float witdh = 150;

    TrainData trainDataRef;

    float trainLength;

    Vector2 size;

    private void Awake()
    {
        trainDataRef = ServiceLocator.Get<TrainData>();
        trainLength = GetTrainLength();
        
    }

    private void Start()
    {
        size.x = trainLength * 1.5f;
        size.y = witdh;
        transform.position = new Vector3(-trainLength/2, 0, 0); 
    }

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

    private float GetTrainLength()
    {
        return Vector3.Distance(Vector3.zero, trainDataRef.GoldBoxPosition.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.y));
        
        if (trainDataRef != null)
            Gizmos.DrawLine(Vector3.zero, trainDataRef.GoldBoxPosition.position);


    }
}