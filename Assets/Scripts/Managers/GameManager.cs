using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base Data")]
    [SerializeField] private LocomotiveStatsSO baseStats;
    [SerializeField] private Texture2D gameplayCursor;
    [SerializeField] private Texture2D menuCursor;

    [SerializeField] private int lastStation = 6;

    public RunResult LastRunResult { get; private set; } = RunResult.None;

    public GameSession Session { get; private set; }

    private bool gameEnded;
    private bool isChangingScene;

    private const string MainMenuScene = "MainMenu";
    private const string ShopScene = "Shop";
    private const string RunScene = "LauScene";
    private const string FinalScene = "FinalScene";

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
    }

    public bool IsFinalStation()
    {
        return Session.SessionConfig.CurrentLevel >= lastStation;
    }

    public int LastStationDebug()
    {
        return lastStation;
    }

    public void GoToStore()
    {
        if (isChangingScene) return;

        Session.SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        ChangeScene(ShopScene, SceneTransitionType.EndingRun);
    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        Session.RebuildStatsSystem();

        ChangeScene(RunScene, SceneTransitionType.StartingRun);
    }

    public void GoToMainMenu()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();

        ChangeScene(MainMenuScene, SceneTransitionType.MainMenu);
    }

    public void Defeat()
    {
        if (isChangingScene) return;
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Defeat;

        Time.timeScale = 1f;

        ChangeScene(FinalScene, SceneTransitionType.Final, instant: true);
        Cursor.SetCursor(gameplayCursor, new Vector2(256, 256), CursorMode.Auto);

    }

    public void Victory()
    {
        if (isChangingScene) return;
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Victory;

        Time.timeScale = 1f;
        Cursor.SetCursor(gameplayCursor, new Vector2(256, 256), CursorMode.Auto);

        ChangeScene(FinalScene, SceneTransitionType.Final);
    }

    public void StartNewSession()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();

        Cursor.SetCursor(gameplayCursor, new Vector2(128, 128), CursorMode.Auto);

        ChangeScene(ShopScene, SceneTransitionType.Generic);
    }

    public void EndSession()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();

        Cursor.SetCursor(menuCursor, new Vector2(256, 256), CursorMode.Auto);

        ChangeScene(MainMenuScene, SceneTransitionType.MainMenu);
    }

    private void ChangeScene(string sceneName, SceneTransitionType transitionType, bool instant = false)
    {
        isChangingScene = true;

        if (instant)
        {
            SceneTransitionManager.Instance.TransitionToSceneInstant(sceneName);
        }
        else
        {
            SceneTransitionManager.Instance.TransitionToScene(sceneName, transitionType);
        }
    }

    public void FinishSceneChange()
    {
        isChangingScene = false;
    }
}