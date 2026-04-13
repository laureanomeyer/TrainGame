using UnityEngine;

public class SouthWagonBrain : WagonBrain
{
    [SerializeField] private float shieldsBonus;

    public override TrainStats GetStatsBuff(LocomotiveStatsSO baseStats)
    {
        return new TrainStats(
            0,
            baseStats.defense * shieldsBonus,
            0,
            0,
            0,
            0,
            0
        );
    }

}
