using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteractions : MonoBehaviour
{

    [SerializeField] private WagonBrain currentWagon;
    [SerializeField] private float repairCapacity;
    [SerializeField] private Slider progressBar;
    [SerializeField] private float currentProgress;
    [SerializeField] private float currentProgressFuel;
    [SerializeField] private float maxAmountProgressBar;
    private float maxAmountProgressBarFuel;

    //[SerializeField] private LocomotiveMovement locomotiveMovement;
    [SerializeField] private LocomotiveBrain locomotiveBrain;
    [SerializeField] private Image fuelFillImage;



    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxAmountProgressBar = 100;
        currentProgress = maxAmountProgressBar;       
    }

    // Update is called once per frame
    void Update()
    {
        //currentProgressFuel = locomotiveMovement.CurrentFuel / locomotiveMovement.MaxFuel;

        if (locomotiveBrain != null)
        {
            fuelFillImage.fillAmount = locomotiveBrain.fuelController.CurrentFuel / locomotiveBrain.fuelController.MaxFuel;
        }
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
            SetUpWagonHP();
        }       
    }

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
  