using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base Data")]
    [SerializeField] private LocomotiveStatsSO baseStats;
    [SerializeField] private Texture2D gameplayCursor;
    [SerializeField] private Texture2D menuCursor;

    public GameSession Session { get; private set; }

    private bool isChangingScene;

    private const string MainMenuScene = "MainMenu";
    private const string ShopScene = "Shop";
    private const string RunScene = "LauScene";

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

    public void GoToStore()
    {
        if (isChangingScene) return;

        
        Session.SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        int currentLevel = Session.SessionConfig.CurrentLevel;

        ChangeScene(ShopScene);
    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        Session.RebuildStatsSystem();
        int currentLevel = Session.SessionConfig.CurrentLevel;

        ChangeScene(RunScene);
    }

    public void GoToMainMenu()
    {
        if (isChangingScene) return;

        ChangeScene(MainMenuScene);
    }

    public void StartNewSession()
    {
        if (isChangingScene) return;

        Session.Reset();
        Session.RebuildStatsSystem();

        ChangeScene(ShopScene);
        Cursor.SetCursor(gameplayCursor, new Vector2(128, 128), CursorMode.Auto);
    }

    public void EndSession()
    {
        if (isChangingScene) return;

        Session.Reset();
        Cursor.SetCursor(menuCursor, new Vector2(256, 256), CursorMode.Auto);
        ChangeScene(MainMenuScene );
    }

    private void ChangeScene(string sceneName)
    {
        isChangingScene = true;

        SceneTransitionManager.Instance.TransitionToScene(sceneName);
    }

    public void FinishSceneChange()
    {
        isChangingScene = false;
    }
}