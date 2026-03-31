using UnityEngine;

public interface ITrainBrain
{
    public void TakeDamage(float damageAmount);

    public void Repair(float repairAmount);

    public void BreakDown();


}
