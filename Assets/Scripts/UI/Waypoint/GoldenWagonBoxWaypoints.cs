using HUDIndicator;
using UnityEngine;

public class GolodenWagonBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOffScreen indicatorOffScreen;
    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOnScreen repairIndicator;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOffScreen.SetRenderer(indicatorRenderer);

        EventBus.Subscribe<OnSetGoldWagonWaypoint>(HandleWaypointVisibility);
        EventBus.Subscribe<OnSetRepairIconEnabledEvent>(HandleRepairVisibility);

        DeactivateWaypointUI();
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetGoldWagonWaypoint>(HandleWaypointVisibility);
        EventBus.Unsubscribe<OnSetRepairIconEnabledEvent>(HandleRepairVisibility);

    }

    public void ActivateWaypointUI()
    {
        indicatorOffScreen.visible = true;
        indicatorOnScreen.visible = true;
    }

    public void DeactivateWaypointUI()
    {
        indicatorOffScreen.visible = false;
        indicatorOnScreen.visible = false;
        repairIndicator.visible = false;
    }

    public void HandleWaypointVisibility(OnSetGoldWagonWaypoint ev)
    {
        indicatorOffScreen.visible = ev.Show;
        indicatorOnScreen.visible = ev.Show;
    }
    public void HandleRepairVisibility(OnSetRepairIconEnabledEvent ev)
    {
        repairIndicator.visible = ev.Enabled;
    }

}
