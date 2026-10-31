using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button settingsButton;

    [Header("References")]
    [SerializeField] private SettingsPanel settingsPanel;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(Quit);
        settingsButton.onClick.AddListener(settingsPanel.Open);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(StartGame);
        quitButton.onClick.RemoveListener(Quit);
        settingsButton.onClick.RemoveListener(settingsPanel.Open);
    }

    private void StartGame()
    {
        AnalyticsManager.Instance.RegisterUserRunPlay();

        GameManager.Instance.StartNewSession();
    }

    private void Quit()
    {
        Application.Quit();
    }
}