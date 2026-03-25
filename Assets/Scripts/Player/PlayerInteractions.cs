using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteractions : MonoBehaviour
{

    [SerializeField] private WagonBrain currentWagon;
    [SerializeField] private float repairCapacity;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Slider progressBarFuel;
    [SerializeField] private float maxAmountProgressBar;
    private float maxAmountProgressBarFuel;
    [SerializeField] private float currentProgress;
    [SerializeField] private float currentProgressFuel;
    [SerializeField] private bool isInWagon;
    [SerializeField] private LocomotiveMovement locomotiveMovement;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isInWagon = false;
        maxAmountProgressBar = 100;
        currentProgress = maxAmountProgressBar;
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.R)) 
        {
            Repair();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }*/
        currentProgressFuel = locomotiveMovement.CurrentFuel / locomotiveMovement.MaxFuel;
        progressBarFuel.value = currentProgressFuel;

        if (currentWagon != null) 
        {
            currentProgress = currentWagon.CurrentHp  / currentWagon.MaxHp;
            progressBar.value = currentProgress;
        }
        
    }

    public void SetUpWagonHP()
    {
        if (currentWagon != null)
        {
            progressBar.value = currentProgress;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Train"))
        {
            other.TryGetComponent<WagonBrain>(out WagonBrain wagon);
            currentWagon = wagon;
            isInWagon = true;
            SetUpWagonHP();
        }
        
    }
    /*private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Train"))
        {
            currentWagon = null;
            Debug.Log("Outside of Wagon");
            isInWagon = false;
        }
    }*/

    public void OnInteract()
    {
        Interact();
    }
    public void OnRepair()
    {
        Repair();
    }
    void Interact()
    {
        if (currentWagon != null) 
        {

        currentWagon.TakeDamage(10);
        }
    }
    void Repair()
    {
        if (currentWagon != null) 
        {
        currentWagon.Repair(repairCapacity);

        }
    }



}
