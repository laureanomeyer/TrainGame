using HUDIndicator;
using UnityEngine;

public class CanvasElementsReferences : MonoBehaviour
{
    [SerializeField] private IndicatorRenderer canvasIndicatorRenderer;
    public static IndicatorRenderer CanvasIndicatorRenderer;

    private void Awake()
    {
        CanvasIndicatorRenderer = canvasIndicatorRenderer;
    }
}
