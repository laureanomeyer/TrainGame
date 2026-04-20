using UnityEngine;

public interface IState 
{
    void Enter(LocomotiveStatsSO stats);
    void Tick();
    void Exit();
}
