using UnityEngine;

public class TutorialScript : MonoBehaviour
{
    [SerializeField] GameObject tutorialText;

    private void Start()
    {
        tutorialText.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialText.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialText.SetActive(false);
        }
    }
}
