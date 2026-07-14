using HUDIndicator;
using System;
using UnityEngine;

public class GoldBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOnScreen.SetRenderer(indicatorRenderer);
        indicatorOffScreen.SetRenderer(indicatorRenderer);

        EventBus.Subscribe<OnTakeGoldEvent>(CallActivateWayPointEvent);
        EventBus.Subscribe<OnDropGoldEvent>(CallDeactivateWayPointEvent);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnTakeGoldEvent>(CallActivateWayPointEvent);
        EventBus.Unsubscribe<OnDropGoldEvent>(CallDeactivateWayPointEvent);
    }

    public void CallActivateWayPointEvent(OnTakeGoldEvent takeGoldEvent)
    {
        ActivateWaypointUI();
    }

    public void CallDeactivateWayPointEvent(OnDropGoldEvent dropGoldEvent)
    {
        DeactivateWaypointUI();
    }

    public void ActivateWaypointUI()
    {
        indicatorOnScreen.visible = true;
        indicatorOffScreen.visible = true;
    }

    public void DeactivateWaypointUI()
    {
        indicatorOnScreen.visible = false;
        indicatorOffScreen.visible = false;
    }

}
