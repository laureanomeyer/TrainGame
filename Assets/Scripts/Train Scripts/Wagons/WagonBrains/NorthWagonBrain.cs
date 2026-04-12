using UnityEngine;

public class NorthWagonBrain : WagonBrain
{
    [SerializeField] private float damageBouns;
    void Awake()
    {
        base.statsBuff = new TrainStats(0,0,0, (GameManager.Instance.TrainData.stats.damageBonus * damageBouns), 0, 0,0);
    }

}
