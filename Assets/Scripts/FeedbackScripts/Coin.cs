using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float arcHeight = 2f;
    Transform target;
    Vector3 startPos;
    float journeyLength;
    float t;

    public void SetTarget(Transform targetTRF)
    {
        target = targetTRF;
        startPos = transform.position;
        journeyLength = Vector3.Distance(startPos, target.position);
        t = 0f;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (target == null) return;

        t += speed * Time.deltaTime / Mathf.Max(journeyLength, 0.01f);
        t = Mathf.Clamp01(t);

        Vector3 basePos = Vector3.Lerp(startPos, target.position, t);
        float height = Mathf.Sin(t * Mathf.PI) * arcHeight; // sube y baja
        transform.position = basePos + Vector3.up * height;

        if (t >= 1f)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
}