using UnityEngine;

public class PlayerBrain : MonoBehaviour
{
    private PlayerInventory inventory;

    public PlayerInventory Inventory => inventory;

    void Awake() //cambie start por awake para orden por si en interactions se hacia antes
    {
        inventory = new PlayerInventory();
    }
}
