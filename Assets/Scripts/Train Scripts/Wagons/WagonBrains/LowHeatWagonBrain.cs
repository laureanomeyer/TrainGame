using UnityEngine;

public class LowHeatWagonBrain : WagonBrain
{
    [SerializeField] private float optimizerBonus;
    void Awake()
    {
        base.statsBuff = new TrainStats(0,0,0, 0, 0, GameManager.Instance.TrainData.stats.damageBonus * optimizerBonus, 0);
    }

}
