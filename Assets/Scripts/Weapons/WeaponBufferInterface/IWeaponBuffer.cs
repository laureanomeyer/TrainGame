using UnityEngine;

public interface IWeaponBuffer
{
    public void UpdateRoFStats();

    public void BufferRoF(float buffer);

    public void DebuffRoF();
}
