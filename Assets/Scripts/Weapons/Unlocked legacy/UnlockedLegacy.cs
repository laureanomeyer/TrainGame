using UnityEngine;

public class UnlockedLegacy
{
    public UnlockedLegacy() 
    {
        //Winchester
        EventBus.Subscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);

        //Spencer
        EventBus.Subscribe<OnUpdatedSpencerLegado>(UpdateSpencer);

        //Coach
        EventBus.Subscribe<OnUpdatedCoachLegado>(UpdateCoach);

        //Colt
        EventBus.Subscribe<OnUpdatedColtLegado>(UpdateColt);
    }

    public int LeftWinchesterPoints = 100;

    public bool UnlockedWinchester = false;

    public bool UnlockedSpencer = false;

    public bool UnlockedCoach = false;

    public bool UnlockedColt = false;

    public void UpdateLeftPointWinchester(OnUpdateWinchesterLegadoLeftPoint updateEvent)
    {
        LeftWinchesterPoints -= updateEvent.point;

        if (LeftWinchesterPoints <= 0)
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

    public void UpdateColt(OnUpdatedColtLegado updateEvent)
    {
        UnlockedColt = true;

        EventBus.Publish(new OnUnlockColtLegado());
    }

    public void UnsuscribeEvents()
    {
        EventBus.Unsubscribe<OnUpdateWinchesterLegadoLeftPoint>(UpdateLeftPointWinchester);
        EventBus.Unsubscribe<OnUpdatedSpencerLegado>(UpdateSpencer);
        EventBus.Unsubscribe<OnUpdatedCoachLegado>(UpdateCoach);
        EventBus.Unsubscribe<OnUpdatedColtLegado>(UpdateColt);
    }
}
