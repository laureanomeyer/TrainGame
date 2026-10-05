using HUDIndicator;
using UnityEngine;

public class GolodenWagonBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOffScreen indicatorOffScreen;
    [SerializeField] private IndicatorOnScreen indicatorOnScreen;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOffScreen.SetRenderer(indicatorRenderer);

        EventBus.Subscribe<OnSetGoldWagonWaypoint>(HandleWaypointVisibility);

        DeactivateWaypointUI();
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetGoldWagonWaypoint>(HandleWaypointVisibility);
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
    }

    public void HandleWaypointVisibility(OnSetGoldWagonWaypoint ev)
    {
        indicatorOffScreen.visible = ev.Show;
        indicatorOnScreen.visible = ev.Show;
    }

}
