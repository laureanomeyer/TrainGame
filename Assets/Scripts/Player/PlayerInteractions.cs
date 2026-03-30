using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private WagonBrain currentWagon;
    [SerializeField] private float repairCapacity;
    [SerializeField] private LocomotiveBrain locomotiveBrain;

    private PlayerInventory playerInventory;
    private PlayerBrain playerBrain;

    public PlayerInventory Inventory => playerInventory;
    public WagonBrain CurrentWagon => currentWagon;
    public LocomotiveBrain LocomotiveBrain => locomotiveBrain;

    void Start()
    {
        playerBrain = GetComponent<PlayerBrain>();
        playerInventory = playerBrain.Inventory;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            other.TryGetComponent(out WagonBrain wagon);
            currentWagon = wagon;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            if (other.TryGetComponent(out WagonBrain wagon) && wagon == currentWagon)
            {
                currentWagon = null;
            }
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