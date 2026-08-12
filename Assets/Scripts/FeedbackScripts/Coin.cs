using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float arcHeight = 2f;

    private float timeInBox = 0.5f;
    private float currentTime = 0;

    private TrailRenderer tr;

    private ArcMover mover;


    public void SetTarget(Transform targetTRF)
    {
        mover = new ArcMover(transform.position, targetTRF, speed, arcHeight);

        if(tr == null)
        {
            tr = GetComponent<TrailRenderer>();
        }

        currentTime = 0;
        ActiveTrail();
    }

    void Update()
    {
        if (mover == null) return;

        if (mover.IsFinished)
        {
            if(currentTime < timeInBox)
            {
                currentTime += Time.deltaTime;
            }
            else
            {
                tr.emitting = false;
                ObjectPoolManager.ReturnObjectToPool(gameObject);
            }
        }
        else
        {
            transform.position = mover.Tick(Time.deltaTime);
        }
    }

    public void ActiveTrail()
    {
        tr.Clear();
        tr.emitting = true;
    }
}