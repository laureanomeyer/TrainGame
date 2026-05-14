using HUDIndicator;
using UnityEngine;

public class CoalBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;

    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOnScreen.SetRenderer(indicatorRenderer);
        indicatorOffScreen.SetRenderer(indicatorRenderer);

        GameEvents.OnTakeFuel += DeactivateWaypointUI;
        GameEvents.OnDropFuel += ActivateWaypointUI;
        TutorialEvents.OnSetCanConsume += SetWaypointVisible;
    }
    public void SetWaypointVisible(bool set)
    {
        indicatorOnScreen.visible = set;
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
        GameEvents.OnTakeFuel -= DeactivateWaypointUI;
        GameEvents.OnDropFuel -= ActivateWaypointUI;
        TutorialEvents.OnSetCanConsume -= SetWaypointVisible;
    }
}
