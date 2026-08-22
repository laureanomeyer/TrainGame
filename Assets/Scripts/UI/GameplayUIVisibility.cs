using UnityEngine;

public class GameplayUIVisibility : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] gameplayCanvases;

    private void OnEnable() => EventBus.Subscribe<OnActivateUiEvent>(OnUiToggled);
    private void OnDisable() => EventBus.Unsubscribe<OnActivateUiEvent>(OnUiToggled);

    private void OnUiToggled(OnActivateUiEvent evt)
    {
        foreach (var cg in gameplayCanvases)
        {
            cg.alpha = evt.Activated ? 1f : 0f;
            cg.interactable = evt.Activated;
            cg.blocksRaycasts = evt.Activated;
        }
    }
}