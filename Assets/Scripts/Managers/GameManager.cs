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

        Cursor.visible = false;

        ChangeScene(ShopScene, SceneTransitionType.EndingRun, GameState.Gameplay);

    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        Session.RebuildStatsSystem();

        Cursor.visible = false;

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
    }
    public void GoToTutorial()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();

        Cursor.visible = false;

        ChangeScene(TutorialScene, SceneTransitionType.Generic, GameState.Tutorial);

    }

    public void Defeat()
    {
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Defeat;

        Time.timeScale = 1f;

        SceneManager.LoadScene(FinalScene);
    }

    public void Victory()
    {
        if (isChangingScene) return;
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Victory;

        Time.timeScale = 1f;

        ChangeScene(FinalScene, SceneTransitionType.Final, GameState.Menu);
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

        Cursor.SetCursor(menuCursor, new Vector2(256, 256), CursorMode.Auto);

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
    }
}