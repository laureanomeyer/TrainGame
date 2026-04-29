using UnityEngine;

public class PlayerInteractionText : MonoBehaviour
{
    [SerializeField] private GameObject interactionText;

    void Start()
    {
        HidePrompt();
    }

    public void ShowPrompt()
    {
        if(interactionText != null)
        {
            interactionText.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if(interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }
}