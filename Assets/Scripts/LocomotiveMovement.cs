using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LocomotiveMovement : MonoBehaviour, IWagon
{
    //[SerializeField] private float baseSpeed;
    [SerializeField] private SharedData speedData;

    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel = 100f;
    [SerializeField] private float fuelUseXSecond = 5f;
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float debugFuelAmount = 20f;
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public bool HasFuel => currentFuel > 0f;

    void Start()
    {
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        //speedData.speed = baseSpeed;
        UpdateSharedSpeed();

    }

    void Update()
    {
        HandleDebugFuel();
        Move();
    }

    void Move()
    {
        if (!HasFuel) 
        {
            speedData.speed = 0f;
            return;
        }
        speedData.speed = baseSpeed;
        transform.position += moveDirection.normalized * speedData.speed * Time.deltaTime;
        ConsumeFuel(fuelUseXSecond * Time.deltaTime);

        
    }

    void Interact()
    {

    }

    /*void ChangeSpeed(float additive)
    {
        baseSpeed = Mathf.Max(0f, baseSpeed + additive);
        //speedData.speed += additive;
        UpdateSharedSpeed();
    }*/

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, amount, maxFuel);
        UpdateSharedSpeed();
        Debug.Log("Current Fuel: " + currentFuel);
    }

    public void RemoveFuel(float amount) //llamar a esta funcion x si hay alguien o algo que te reste nafta 
    {
        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, maxFuel);
        UpdateSharedSpeed();
        Debug.Log("Current Fuel: " + currentFuel);
    }

    private void ConsumeFuel(float amount) //gasto natural del tren
    {
        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, maxFuel);
        UpdateSharedSpeed();
    }

    private void UpdateSharedSpeed()
    {
        if (HasFuel) 
        {
            speedData.speed = baseSpeed;
        }
        else
        {
            speedData.speed = 0f;
        }
    }

    private void HandleDebugFuel()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            AddFuel(debugFuelAmount);
        }
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            RemoveFuel(debugFuelAmount);
        }
    }
}
