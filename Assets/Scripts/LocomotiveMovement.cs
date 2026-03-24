using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LocomotiveMovement : MonoBehaviour, IWagon
{
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel = 100f;
    [SerializeField] private float fuelOptimizer = 1;
    [SerializeField] private float fuelUseXSecond;
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    [SerializeField] private float baseSpeed = 5f;
    private float actualSpeed;
    [SerializeField] private float debugFuelAmount = 20f;
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public bool HasFuel => currentFuel > 0f;

    void Start()
    {
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        GameManager.Instance.SetSpeed(baseSpeed);
        actualSpeed = baseSpeed;
        UpdateSharedSpeed();

       fuelUseXSecond = actualSpeed / ( 2 * fuelOptimizer);
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
            GameManager.Instance.SetSpeed(0);
            return;
        }
        ConsumeFuel(fuelUseXSecond * Time.deltaTime);  
    }

    void Interact()
    {

    }

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

    private void ModifySpeed(float speedToAdd)
    {
        actualSpeed += speedToAdd;
        fuelUseXSecond = actualSpeed / (2 * fuelOptimizer);
        UpdateSharedSpeed();
    }

    private void UpdateSharedSpeed()
    {
        if (HasFuel) 
        {
            GameManager.Instance.SetSpeed(actualSpeed);
        }
        else
        {
            GameManager.Instance.SetSpeed(0);
        }
    }

    private void HandleDebugFuel()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            AddFuel(debugFuelAmount);
            ModifySpeed(10);
        }
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            RemoveFuel(debugFuelAmount);
        }
    }
}
