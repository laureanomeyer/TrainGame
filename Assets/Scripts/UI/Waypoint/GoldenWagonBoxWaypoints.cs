using HUDIndicator;
using UnityEngine;

public class GolodenWagonBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOffScreen.SetRenderer(indicatorRenderer);

        GameEvents.OnTakeGold += DeactivateWaypointUI;
        GameEvents.OnDropGold += ActivateWaypointUI;
    }

    public void ActivateWaypointUI()
    {
        indicatorOffScreen.visible = true;
    }

    public void DeactivateWaypointUI()
    {
        indicatorOffScreen.visible = false;
    }

    private void OnDestroy()
    {
        GameEvents.OnTakeGold -= DeactivateWaypointUI;
        GameEvents.OnDropGold -= ActivateWaypointUI;
    }
}
