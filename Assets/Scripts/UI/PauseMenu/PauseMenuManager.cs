using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;

    [Header("Scenes")]
    [SerializeField] private string[] pausableScenes = { "LauScene", "Shop" };
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private InputAction pauseAction;
    private bool isPaused;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        pauseAction = InputSystem.actions.FindAction("Pause");
    }

    private void OnEnable()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePressed;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePressed;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        if (!IsInPausableScene())
        {
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void QuitToMainMenu()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (!IsInPausableScene())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private bool IsInPausableScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        foreach (string sceneName in pausableScenes)
        {
            if (currentSceneName == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}