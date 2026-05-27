using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base Data")]
    [SerializeField] private LocomotiveStatsSO baseStats;
    [SerializeField] private Texture2D menuCursor;

    [SerializeField] private int lastStation = 6;

    public RunResult LastRunResult { get; private set; } = RunResult.None;
    public GameState CurrentState { get; private set; }

    public GameSession Session { get; private set; }

    private bool gameEnded;
    private bool isChangingScene;

    private GameState stateAfterTransition;
    public bool IsGameplayState => CurrentState == GameState.Gameplay || CurrentState == GameState.Tutorial;
    public bool IsTransitioning => CurrentState == GameState.Transition;
    public bool IsTutorial => SceneManager.GetActiveScene().name == "TutorialScene";
    public bool IsGameplayScene => SceneManager.GetActiveScene().name == "TutorialScene" || SceneManager.GetActiveScene().name == "LauScene" || SceneManager.GetActiveScene().name == "Shop";

    private const string MainMenuScene = "MainMenu";
    private const string ShopScene = "Shop";
    private const string RunScene = "LauScene";
    private const string FinalScene = "FinalScene";
    private const string TutorialScene = "TutorialScene";

    private void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        Session = new GameSession(baseStats);

        CurrentState = GameState.Menu;
        stateAfterTransition = GameState.Menu;

        GameEvents.OnShowCursor += ShowRealCursor;
    }
    private void OnDestroy()
    {
        GameEvents.OnShowCursor -= ShowRealCursor;
    }

    public bool IsFinalStation()
    {
        return Session.SessionConfig.CurrentLevel >= lastStation;
    }

    public void GoToStore()
    {
        if (isChangingScene) return;

        Session.SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        ChangeScene(ShopScene, SceneTransitionType.EndingRun, GameState.Gameplay);
    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        Session.RebuildStatsSystem();

        ChangeScene(RunScene, SceneTransitionType.StartingRun, GameState.Gameplay);
    }

    public void GoToMainMenu()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();

        ChangeScene(MainMenuScene, SceneTransitionType.MainMenu, GameState.Menu);
        Cursor.visible = true;
    }
    public void GoToTutorial()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();

        ChangeScene(TutorialScene, SceneTransitionType.Generic, GameState.Tutorial);

    }

    public void Defeat()
    {
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Defeat;

        Time.timeScale = 1f;

        SceneManager.LoadScene(FinalScene);
        Cursor.visible = true;
    }

    public void Victory()
    {
        if (isChangingScene) return;
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Victory;

        Time.timeScale = 1f;

        ChangeScene(FinalScene, SceneTransitionType.Final, GameState.Menu);
        Cursor.visible = true;
    }

    public void StartNewSession()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();

        ChangeScene(ShopScene, SceneTransitionType.Generic, GameState.Gameplay);
    }

    public void EndSession()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();

        ChangeScene(MainMenuScene, SceneTransitionType.MainMenu, GameState.Menu);
    }

    private void ChangeScene(string sceneName, SceneTransitionType transitionType, GameState nextState)
    {
        isChangingScene = true;

        stateAfterTransition = nextState;
        CurrentState = GameState.Transition;

        SceneTransitionManager.Instance.TransitionToScene(sceneName, transitionType);

        
    }

    public void FinishSceneChange()
    {
        isChangingScene = false;
        CurrentState = stateAfterTransition;
        if (IsGameplayScene) Cursor.visible = false;
        else Cursor.visible = true;
    }

    private void ShowRealCursor(bool show)
    {
        Cursor.visible = !show;
    }
}