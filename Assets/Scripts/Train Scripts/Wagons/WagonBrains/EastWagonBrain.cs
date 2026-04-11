using UnityEngine;

public class EastWagonBrain : WagonBrain
{
    [SerializeField] private float hpBonus;
    new void Awake()
    {
        base.statsBuff = new TrainStats(0,(GameManager.Instance.TrainData.stats.trainMaxHp * hpBonus),0,0, 0, 0, 0,0);
    }

}
