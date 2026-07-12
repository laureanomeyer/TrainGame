using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base Data")]
    [SerializeField] private LocomotiveStatsSO baseStats;
    [SerializeField] private LocomotiveStatsSO baseMultStats;

    [SerializeField] private int lastStation = 6;

    [Header("Stations")]
    [SerializeField] private StationRouteSO stationRoute;

    public RunResult LastRunResult { get; private set; } = RunResult.None;
    public GameState CurrentState { get; private set; }
    public GameSession Session { get; private set; }

    private bool gameEnded;
    private bool isChangingScene;
    private CursorType currentCursor;

    private GameState stateAfterTransition;
    public bool IsGameplayState => CurrentState == GameState.Gameplay || CurrentState == GameState.Tutorial;
    public bool IsTransitioning => CurrentState == GameState.Transition;
    public bool IsTutorial => SceneManager.GetActiveScene().name == "TutorialScene";
    public bool IsGameplayScene => SceneManager.GetActiveScene().name == "TutorialScene" || SceneManager.GetActiveScene().name == "LauScene" || SceneManager.GetActiveScene().name == "Shop";
    public bool IsInShop => SceneManager.GetActiveScene().name == "Shop";
    public bool IsInCombat => SceneManager.GetActiveScene().name == "TutorialScene" || SceneManager.GetActiveScene().name == "LauScene";

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

        CurrentState = GameState.Menu;
        stateAfterTransition = GameState.Menu;

        Session = new GameSession(baseStats, baseMultStats);

        GameEvents.OnShowCursor += ShowCursor;
    }
    private void OnDestroy()
    {
        GameEvents.OnShowCursor -= ShowCursor;
    }

    public bool IsFinalStation()
    {
        return Session._SessionConfig.CurrentLevel >= lastStation;
    }

    public string GetCurrentStationName() //STATIONNAMES
    {
        if (stationRoute == null)
        {
            return "Estación desconocida";
        }

        int currentLevel = 1;

        if (Session != null && Session._SessionConfig != null)
        {
            currentLevel = Session._SessionConfig.CurrentLevel;
        }

        return stationRoute.GetStationNameByLevel(currentLevel);
    }

    public string GetStationNameByLevel(int level)
    {
        if (stationRoute == null)
        {
            return "Estación desconocida";
        }

        return stationRoute.GetStationNameByLevel(level);
    }


    public void EnterTransitionState()
    {
        CurrentState = GameState.Transition;
    }

    public void GoToStore()
    {
        if (isChangingScene) return;

        Session._SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        ChangeScene(ShopScene, SceneTransitionType.EndingRun, GameState.Gameplay);
        ShowCursor(CursorType.Gameplay);
    }
    public void SkipRun()
    {
        Session._SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        ChangeScene(ShopScene, SceneTransitionType.EndingRun, GameState.Gameplay);
    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        Session.RebuildStatsSystem();

        ChangeScene(RunScene, SceneTransitionType.StartingRun, GameState.Gameplay);
        ShowCursor(CursorType.Gameplay);
    }

    public void GoToMainMenu()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;


        ChangeScene(MainMenuScene, SceneTransitionType.MainMenu, GameState.Menu);
        ShowCursor(CursorType.Real);

        EndSession();
    }
    public void GoToTutorial()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.RebuildStatsSystem();

        ChangeScene(TutorialScene, SceneTransitionType.Generic, GameState.Tutorial);
        ShowCursor(CursorType.Gameplay);
    }

    public void Defeat()
    {
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Defeat;

        Time.timeScale = 1f;

        SceneManager.LoadScene(FinalScene);
        ShowCursor(CursorType.Real);
    }

    public void Victory()
    {
        if (isChangingScene) return;
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Victory;

        Time.timeScale = 1f;

        ChangeScene(FinalScene, SceneTransitionType.Final, GameState.Menu);
        ShowCursor(CursorType.Real);
    }

    public void StartNewSession()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();
        Session = new GameSession(baseStats, baseMultStats);

        ChangeScene(ShopScene, SceneTransitionType.Generic, GameState.Gameplay);
    }

    public void EndSession()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();
        Session = new GameSession(baseStats, baseMultStats);
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

        if (IsTutorial) ShowCursor(CursorType.Gameplay);
        else if (IsGameplayScene) ShowCursor(CursorType.Gameplay);
        else ShowCursor(CursorType.Real);

        if (IsInCombat) MusicManager.Instance.SetGameplayMusic();
        else if (IsInShop) MusicManager.Instance.SetStoreMusic();
        else MusicManager.Instance.SetMenuMusic();
    }
    public CursorType GetCurrentCursor()
    {
        Debug.Log(currentCursor.ToString());
        return currentCursor;
    }
    private void ShowCursor(CursorType cursor)
    {
        currentCursor = cursor;

        switch (cursor) 
        {
            case CursorType.Real: 
                Cursor.visible = true;
                GameEvents.ShowGameplayCursor(false);
                break;

            case CursorType.Gameplay: 
                Cursor.visible = false; 
                GameEvents.ShowGameplayCursor(true); 
                break;
            case CursorType.Hidden: 
                Cursor.visible = false;
                GameEvents.ShowGameplayCursor(false);
                break;
        }

    }
}