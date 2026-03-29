using UnityEngine;

public class PlayerBrain : MonoBehaviour
{
    private PlayerInventory inventory;

    public PlayerInventory Inventory => inventory;

    void Start()
    {
        inventory = new PlayerInventory();
    }

    void Update()
    {
        
    }
}
