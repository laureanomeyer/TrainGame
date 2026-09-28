using UnityEngine;

public interface IWeaponBuffer
{
    public void BufferRoF(float buffer);

    public void DebuffRoF();

    public void BufferDamage(float buffer);

    public void DebuffDamage();
}
