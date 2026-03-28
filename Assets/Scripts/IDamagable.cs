using UnityEngine;

public interface IDamagable 
{
   void TakeDamage(float damageToTake) { }
   void Repair(float deltaTime, float repairAmount) { }
   void Break() { }
}

