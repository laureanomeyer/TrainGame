using UnityEngine;

public class SouthWagonBrain : WagonBrain
{
    [SerializeField] private float shieldsBouns;
    new void Awake()
    {
        base.statsBuff = new TrainStats(0,0, (GameManager.Instance.TrainData.stats.shields * shieldsBouns), 0, 0, 0, 0,0);
    }

}
