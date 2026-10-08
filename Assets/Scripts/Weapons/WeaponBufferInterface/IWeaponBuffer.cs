using UnityEngine;

public interface IWeaponBuffer
{
    public void BufferRoF(float buffer);

    public void DebuffRoF(float buffer);

    public void BufferDamage(float buffer);

    public void DebuffDamage(float buffer);
}
