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

        GameEvents.OnTakeGold += ActivateWaypointUI;
        GameEvents.OnDropGold += DeactivateWaypointUI;
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

    private void OnDestroy()
    {
        GameEvents.OnTakeGold -= ActivateWaypointUI;
        GameEvents.OnDropGold -= DeactivateWaypointUI;
    }
}
