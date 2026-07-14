using UnityEngine;

public class PlayerInteractionText : MonoBehaviour
{
    [SerializeField] private GameObject interactionText;

    void Start()
    {
        HidePrompt();
        EventBus.Subscribe<OnShowInteractEvent>(ShowPrompt);
        EventBus.Subscribe<OnHideInteractEvent>(CallHidePromptEvent);
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnShowInteractEvent>(ShowPrompt);
        EventBus.Unsubscribe<OnHideInteractEvent>(CallHidePromptEvent);
    }

    public void ShowPrompt(OnShowInteractEvent showInteractEvent)
    {
        if(interactionText != null)
        {
            interactionText.SetActive(true);
        }
    }

    public void CallHidePromptEvent(OnHideInteractEvent hideInteractEvent)
    {
        HidePrompt();
    }

    public void HidePrompt()
    {
        if(interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

}