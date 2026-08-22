using UnityEngine;

public class UnlockedLegacy
{
    public UnlockedLegacy() 
    {
        //Winchester
        EventBus.Subscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);

        //Spencer
        EventBus.Subscribe<OnUpdatedSpencerLegado>(UpdateSpencer);

        //Spencer
        EventBus.Subscribe<OnUpdatedCoachLegado>(UpdateCoach);
    }

    public int LeftPoints = 100;

    public bool UnlockedWinchester = false;

    public bool UnlockedSpencer = false;

    public bool UnlockedCoach = false;

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

    public void UpdateCoach(OnUpdatedCoachLegado updateEvent)
    {
        UnlockedCoach = true;

        EventBus.Publish(new OnUnlockCoachLegado());
    }

    public void UnsuscribeEvents()
    {
        EventBus.Unsubscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);
        EventBus.Unsubscribe<OnUpdatedSpencerLegado>(UpdateSpencer);
        EventBus.Unsubscribe<OnUpdatedCoachLegado>(UpdateCoach);
    }
}
