using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocomotiveMovement : MonoBehaviour, IWagon
{
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float currentFuel = 100f;
    [SerializeField] private float fuelOptimizer = 1;
    [SerializeField] private float fuelUseXSecond;
    [SerializeField] private float baseSpeed = 5f;
    private float actualSpeed;
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public bool HasFuel => currentFuel > 0f;

    public Transform Transform => transform;


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
        Debug.Log(currentFuel);
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

}
