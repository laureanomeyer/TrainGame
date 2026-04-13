using UnityEngine;

//Interfaz utilizada para vagones que poseen mejoras pasivas de cualquier tipo

public interface IBuffer
{
    //Alamacena los buffers para los trainstats de train data
    TrainStats GetStatsBuff(LocomotiveStatsSO baseStats);
}
