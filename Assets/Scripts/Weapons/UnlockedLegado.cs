using UnityEngine;

public class UnlockedLegado
{
    public UnlockedLegado() 
    {
        EventBus.Subscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);
    }

    public int LeftPoints = 1;

    public bool UnlockedWinchester = false;

    public void UpdateLeftPointWinchester(OnUpdateWinchesterLegadoLeftPoint updateEvent)
    {
        LeftPoints -= updateEvent.point;

        if (LeftPoints <= 0)
        {
            UnlockedWinchester = true;

            EventBus.Publish(new OnUnlockWinchesterLegado());
        }
    }

    public void UnsuscribeEvents()
    {
        EventBus.Unsubscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);
    }
}
