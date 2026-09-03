using UnityEngine;

public static class BehaviorFactory
{
    public static IInteractableWithInventory Create (InteractableType type, BoxCollider collider)
    {
        return type switch
        {
            InteractableType.GoldBox => new GoldBox(collider),
            InteractableType.CoalBox => new CoalBox(collider),

            _ => null
        };
    }
}

