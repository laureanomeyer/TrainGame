using UnityEngine;

public class PlayerInteractionText : MonoBehaviour
{

    [SerializeField] private GameObject interactionText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HidePrompt();
    }

    // Update is called once per frame
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