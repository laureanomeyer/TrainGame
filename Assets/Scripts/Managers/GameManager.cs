using UnityEditor;
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

    [Header("Cursor")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotSpot;

    public RunResult LastRunResult { get; private set; } = RunResult.None;
    public GameState CurrentState { get; private set; }
    public GameSession Session { get; private set; }

    private bool gameEnded;
    private bool isChangingScene;
    private CursorType currentCursor;

    private GameState stateAfterTransition;
    public bool IsGameplayState => CurrentState == GameState.Gameplay || CurrentState == GameState.Tutorial;
    public bool IsTransitioning => CurrentState == GameState.Transition;
    public bool IsTutorial => SceneManager.GetActiveScene().name == "SceneTutorial";
    public bool IsGameplayScene => SceneManager.GetActiveScene().name == "SceneTutorial" || SceneManager.GetActiveScene().name == "SceneLau" || SceneManager.GetActiveScene().name == "SceneShop";
    public bool IsInShop => SceneManager.GetActiveScene().name == "SceneShop";
    public bool IsInCombat => SceneManager.GetActiveScene().name == "SceneTutorial" || SceneManager.GetActiveScene().name == "SceneLau";

    private const string SceneMainMenu = "SceneMainMenu";
    private const string SceneShop = "SceneShop";
    private const string SceneRun = "SceneLau";
    private const string SceneFinal = "SceneFinal";
    private const string SceneTutorial = "SceneTutorial";

    private void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        PlayerPrefs.SetInt("TutorialCompleted", 0);

        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        CurrentState = GameState.Menu;
        stateAfterTransition = GameState.Menu;

        Session = new GameSession(baseStats, baseMultStats);

        EventBus.Subscribe<OnShowCursorEvent>(ChangeCursorEvent);

        Cursor.SetCursor( cursorTexture, hotSpot, CursorMode.ForceSoftware );
    }
    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnShowCursorEvent>(ChangeCursorEvent);
    }

    public void ChangeGameState(GameState state)
    {
        CurrentState = state;
    }

    public bool IsFinalStation()
    {
        return Session._SessionConfig.CurrentLevel >= lastStation;
    }

    public int GetCurrentLevel()
    {
        if (Session != null && Session._SessionConfig != null)
        {
            return Session._SessionConfig.CurrentLevel;
        }
        return 1;
    }

    public string GetCurrentStationName() //STATIONNAMES
    {
        if (stationRoute == null)
        {
            return "Unknown station";
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
            return "Unknown station";
        }

        return stationRoute.GetStationNameByLevel(level);
    }

    public void EnterTransitionState()
    {
        CurrentState = GameState.Transition;
    }

    public void SkipRun()
    {
        Session._SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        ChangeScene(SceneShop, SceneTransitionType.EndingRun, GameState.UI, CursorType.Real);
    }

    public void GoToStore()
    {
        if (isChangingScene) return;

        Session._SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        ChangeScene(SceneShop, SceneTransitionType.EndingRun, GameState.UI, CursorType.Real);
        ShowCursor(CursorType.Gameplay);
    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        Session.RebuildStatsSystem();

        ChangeScene(SceneRun, SceneTransitionType.StartingRun, GameState.Gameplay, CursorType.Gameplay);
        ShowCursor(CursorType.Gameplay);
    }

    public void GoToMainMenu()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        ChangeScene(SceneMainMenu, SceneTransitionType.MainMenu, GameState.Menu, CursorType.Real);
        ShowCursor(CursorType.Real);

        EndSession();
    }
    public void GoToTutorial()
    {
        if (isChangingScene) return;

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.RebuildStatsSystem();

        ChangeScene(SceneTutorial, SceneTransitionType.Generic, GameState.Tutorial, CursorType.HiddenAndFrozen);
    }

    public void Defeat()
    {
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Defeat;

        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneFinal);
        ShowCursor(CursorType.Real);
    }

    public void Victory()
    {
        if (isChangingScene) return;
        if (gameEnded) return;

        gameEnded = true;
        LastRunResult = RunResult.Victory;

        Time.timeScale = 1f;

        ChangeScene(SceneFinal, SceneTransitionType.Final, GameState.Menu, CursorType.Real);
        ShowCursor(CursorType.Real);
    }

    public void StartNewSession()
    {
        if (isChangingScene) 
        {
            Debug.Log("Cambiando de escena");
            return;
        }

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();
        Session = new GameSession(baseStats, baseMultStats);

        if (PlayerPrefs.GetInt("TutorialCompleted") != 0) GoToRun();
        else GoToTutorial();
    }

    public void EndSession()
    {

        gameEnded = false;
        LastRunResult = RunResult.None;

        Session.Reset();
        Session.RebuildStatsSystem();
        Session = new GameSession(baseStats, baseMultStats);
    }

    private void ChangeScene(string sceneName, SceneTransitionType transitionType, GameState nextState, CursorType nextSceneCursor)
    {
        isChangingScene = true;

        stateAfterTransition = nextState;
        CurrentState = GameState.Transition;

        SceneTransitionManager.Instance.TransitionToScene(sceneName, transitionType, nextSceneCursor);    
    }

    public void FinishSceneChange(CursorType newSceneCursor)
    {
        isChangingScene = false;
        CurrentState = stateAfterTransition;

        if (IsInCombat) MusicManager.Instance.SetGameplayMusic();
        else if (IsInShop) MusicManager.Instance.SetStoreMusic();
        else MusicManager.Instance.SetMenuMusic();

        currentCursor = newSceneCursor;
    }
    public CursorType GetCurrentCursor()
    {
        return currentCursor;
    }

    public void ChangeCursorEvent(OnShowCursorEvent showCursorEvent)
    {
        ShowCursor(showCursorEvent.Cursor);
    }

    private void ShowCursor(CursorType cursor)
    {
        currentCursor = cursor;

        switch (cursor) 
        {
            case CursorType.Real: 
                Cursor.visible = true;
                EventBus.Publish(new OnShowGameplayCursorEvent(false));
                break;
            case CursorType.Gameplay: 
                Cursor.visible = false;
                EventBus.Publish(new OnShowGameplayCursorEvent(true));
                break;
            case CursorType.HiddenAndFrozen: 
                Cursor.visible = false;
                EventBus.Publish(new OnShowGameplayCursorEvent(false));
                break;
            case CursorType.HiddenAndMoveable:
                Cursor.visible = false;
                EventBus.Publish(new OnShowGameplayCursorEvent(false));
                break;
        }

    }
}