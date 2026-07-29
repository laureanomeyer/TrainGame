using UnityEngine;

public class OnWagonAddedToDisplayEvent : IGameEvent
{
    public string AnchorKey;
    public OnWagonAddedToDisplayEvent(string anchorKey)
    {
        AnchorKey = anchorKey;
    }
}
