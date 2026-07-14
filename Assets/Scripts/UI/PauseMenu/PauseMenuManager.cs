using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;

    [Header("Scenes")]
    [SerializeField] private string[] pausableScenes = { "LauScene", "Shop", "TutorialScene" };
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private InputAction pauseAction;
    private bool isPaused;
    private CursorType currentCursor;

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

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(QuitToMainMenu);
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
            resumeButton.onClick.RemoveAllListeners();
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
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
        currentCursor = GameManager.Instance.GetCurrentCursor();

        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        EventBus.Publish(new OnHideInteractEvent());
        EventBus.Publish(new OnShowCursorEvent(CursorType.Real));
        EventBus.Publish(new OnActivateUiEvent(false));
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        EventBus.Publish(new OnShowCursorEvent(currentCursor));

        Time.timeScale = 1f;
        isPaused = false;
        EventBus.Publish(new OnActivateUiEvent(true));
    }

    public void QuitToMainMenu()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        GameManager.Instance.GoToMainMenu();
        Cursor.visible = true;

        MusicManager.Instance.SetMenuMusic();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
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