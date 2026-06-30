using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    //[SerializeField] private Button tutorialButton;
    private void Start()
    {
        startButton.onClick.AddListener(GameManager.Instance.GoToTutorial);
        quitButton.onClick.AddListener(Quit);
        //tutorialButton.onClick.AddListener(GameManager.Instance.GoToTutorial);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        //tutorialButton.onClick.RemoveAllListeners();
    }

    private void Quit()
    {
        Application.Quit();
    }
}
