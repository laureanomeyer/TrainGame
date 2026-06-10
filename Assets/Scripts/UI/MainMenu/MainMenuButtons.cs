using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] private Button startButton;
    //[SerializeField] private Button tutorialButton;
    private void Start()
    {
        startButton.onClick.AddListener(GameManager.Instance.GoToTutorial);
        //tutorialButton.onClick.AddListener(GameManager.Instance.GoToTutorial);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
        //tutorialButton.onClick.RemoveAllListeners();
    }
}
