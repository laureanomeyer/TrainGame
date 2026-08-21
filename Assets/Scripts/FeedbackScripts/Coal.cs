using UnityEngine;

public class Coal : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] float arcHeight = 15f;

    private ArcMover mover;

    public void SetTarget(Transform targetTRF)
    {
        mover = new ArcMover(transform.position, targetTRF, speed, arcHeight);
    }

    void Update()
    {
        if (mover == null) return;

        transform.position = mover.Tick(Time.deltaTime);

        if (mover.IsFinished)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
        
    }
}