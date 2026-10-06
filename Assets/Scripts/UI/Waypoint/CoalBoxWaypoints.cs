using HUDIndicator;
using UnityEngine;

public class CoalBoxWaypoints : MonoBehaviour, IWaypointsUI
{
    private IndicatorRenderer indicatorRenderer;

    [SerializeField] private IndicatorOnScreen indicatorOnScreen;
    [SerializeField] private IndicatorOffScreen indicatorOffScreen;


    void Awake()
    {
        EventBus.Subscribe<OnSetCoalWaypoint>(HandleWaypointVisibility); //Prende y apaga segun la cantidad de carbon

    }
    void Start()
    {
        indicatorRenderer = CanvasElementsReferences.CanvasIndicatorRenderer;

        indicatorOnScreen.SetRenderer(indicatorRenderer);
        indicatorOffScreen.SetRenderer(indicatorRenderer);

        DeactivateWaypointUI();
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetCoalWaypoint>(HandleWaypointVisibility);
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

    public void HandleWaypointVisibility(OnSetCoalWaypoint ev)
    {
        indicatorOffScreen.visible = ev.Show;
        indicatorOnScreen.visible = ev.Show;
    }

}
