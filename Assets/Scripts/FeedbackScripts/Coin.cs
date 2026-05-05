using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;

public class Coin : MonoBehaviour
{
    float speed = 5f;
    Transform target;

    void Update()
    {
        Move();
    }

    public void SetTarget(Transform targetTRF) 
    {
        target = targetTRF;
    }

    private void Move()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position,target.position, speed * Time.deltaTime);
        if(transform.position == target.position )
        {
            Destroy(gameObject);
        }
    }

}

