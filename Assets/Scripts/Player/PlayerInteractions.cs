using Unity.VisualScripting;
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
    [SerializeField] private LocomotiveBrain locomotiveBrain;
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Image fuelMaxCapacityImage;
    [SerializeField] private Image shieldImage;

    private float fuelMaxCapacity;

    [SerializeField] private Image coalImage;

    private PlayerInventory playerInventory;
    private PlayerBrain playerBrain;    

    public PlayerInventory Inventory => playerInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        maxAmountProgressBar = 100;
        currentProgress = maxAmountProgressBar;

        playerBrain = GetComponent<PlayerBrain>();

        playerInventory = playerBrain.Inventory;

        UpdateCoalUI();

    }

    // Update is called once per frame
    void Update()
    {

        if (locomotiveBrain != null)
        {
            fuelFillImage.fillAmount = locomotiveBrain.fuelController.CurrentFuel / locomotiveBrain.fuelController.CurrentMaxFuel;
            fuelMaxCapacityImage.fillAmount = locomotiveBrain.fuelController.CurrentMaxFuel / locomotiveBrain.fuelController.FuelMaxCapaciy;
            shieldImage.fillAmount = locomotiveBrain.fuelController.CurrentShield / locomotiveBrain.fuelController.MaxShield;
        }
        if (currentWagon != null) 
        {
            currentProgress = currentWagon.CurrentHp  / currentWagon.MaxHp;
            progressBar.value = currentProgress;
        }    

        UpdateCoalUI(); 
    }


    void UpdateCoalUI()
    {
        if (playerInventory != null)
        {
            if (playerInventory.HasCoal) 
            {
                coalImage.color = Color.black;
            }
            else 
            { 
                coalImage.color = Color.white;
            }
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
  