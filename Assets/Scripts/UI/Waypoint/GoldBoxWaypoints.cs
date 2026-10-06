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

        EventBus.Subscribe<OnSetGoldWaypoint>(HandleWaypointVisibility);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetGoldWaypoint>(HandleWaypointVisibility);
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

    public void HandleWaypointVisibility(OnSetGoldWaypoint ev)
    {
        indicatorOnScreen.visible = ev.Show;
        indicatorOffScreen.visible = ev.Show;
    }

}
