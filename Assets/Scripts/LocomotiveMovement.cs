using System;
using UnityEngine;

public class LocomotiveMovement : MonoBehaviour, IWagon
{
    [SerializeField] private float baseSpeed;
    [SerializeField] private SharedData speedData;

    void Start()
    {
        speedData.speed = baseSpeed;
    }

    void Update()
    {

    }

    void Move()
    {
    }

    void Interact()
    {

    }

    void ChangeSpeed(float additive)
    {
        speedData.speed += additive;
    }
}
