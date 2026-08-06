using UnityEngine;

public class UnlockedLegado
{
    public UnlockedLegado() 
    {
        EventBus.Subscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);
        EventBus.Subscribe<OnUpdatedSpencerLegado>(UpdateSpencer);
    }

    public int LeftPoints = 1;

    public bool UnlockedWinchester = false;

    public bool UnlockedSpencer = false;

    public void UpdateLeftPointWinchester(OnUpdateWinchesterLegadoLeftPoint updateEvent)
    {
        LeftPoints -= updateEvent.point;

        if (LeftPoints <= 0)
        {
            UnlockedWinchester = true;

            EventBus.Publish(new OnUnlockWinchesterLegado());
        }
    }

    public void UpdateSpencer(OnUpdatedSpencerLegado updateEvent)
    {
        UnlockedSpencer = true;

        EventBus.Publish(new OnUnlockSpencerLegado());
    }

    public void UnsuscribeEvents()
    {
        EventBus.Unsubscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);
        EventBus.Unsubscribe<OnUpdatedSpencerLegado>(UpdateSpencer);
    }
}
