using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{

    [SerializeField] private WagonBrain currentWagon;
    [SerializeField] private float repairCapacity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Train"))
        {
            other.TryGetComponent<WagonBrain>(out WagonBrain wagon);
            currentWagon = wagon;
            Debug.Log("In Wagon");
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Train"))
        {
            currentWagon = null;
            Debug.Log("Outside of Wagon");
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
        Debug.Log("Interact");
        currentWagon.TakeDamage(10);
        }
    }
    void Repair()
    {
        if (currentWagon != null) 
        {
        currentWagon.Repair(repairCapacity);
        Debug.Log("InteractR"); 
        }
    }



}
