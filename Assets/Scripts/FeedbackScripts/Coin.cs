using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float arcHeight = 2f;

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
            ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}