using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;


public class FuelCharger: MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private LocomotiveBrain locomotive;
    InteractInputHandler inputHandler;
    PlayerBrain playerRef;

    void Start()
    {
        inputActions.Enable();
        var interactAction = inputActions.FindAction("Player/Interact");
        inputHandler = new InteractInputHandler(interactAction, OnInteract);
    }

    void OnInteract()
    {
        if (playerRef != null)
            AddFuel();
    }
   
    private void AddFuel()
    {
        if (playerRef.Inventory.HasCoal) 
        {
            locomotive.AddFuel();
            playerRef.Inventory.DepositCoal();
            GameEvents.DropFuel();
        }else return;
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.TryGetComponent<PlayerBrain>(out playerRef);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerRef = null;
        }
    }
    private void OnDestroy()
    {
        inputHandler.Dispose();
    }

}

