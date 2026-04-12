using UnityEngine;

public class EastWagonBrain : WagonBrain
{
    [SerializeField] private float hpBonus;
     void Awake()
    {
        base.statsBuff = new TrainStats((GameManager.Instance.TrainData.stats.trainMaxHp * hpBonus),0,0, 0, 0, 0,0);
    }

}
