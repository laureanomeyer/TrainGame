using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum InteractableType { CoalBox, GoldBox}

public static class BehaviorFactory
{
    public static IInteractable Create (InteractableType type)
    {
        return type switch
        {
            InteractableType.GoldBox => new GoldBox(),
            InteractableType.CoalBox => new CoalBox(),

            _ => null
        };
    }
}

