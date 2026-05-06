using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base Data")]
    [SerializeField] private LocomotiveStatsSO baseStats;

    public GameSession Session { get; private set; }

    private bool isChangingScene;

    private const string MainMenuScene = "MainMenu";
    private const string ShopScene = "Shop";
    private const string RunScene = "LauScene";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Session = new GameSession(baseStats);
    }

    public void GoToStore()
    {
        if (isChangingScene) return;

        /*
         * La run terminó.
         * Entonces avanzamos al siguiente nivel/estación.
         */
        Session.SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        int currentLevel = Session.SessionConfig.CurrentLevel;

        ChangeScene(
            ShopScene,
            "Llegando a Estación " + currentLevel
        );
    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        /*
         * Salimos del shop y entramos a la run actual.
         * No avanzamos el nivel acá, porque ya avanzó al llegar al shop.
         */
        Session.RebuildStatsSystem();

        int currentLevel = Session.SessionConfig.CurrentLevel;

        ChangeScene(
            RunScene,
            "Partiendo al Nivel " + currentLevel
        );
    }

    public void GoToMainMenu()
    {
        if (isChangingScene) return;

        ChangeScene(
            MainMenuScene,
            "Volviendo al menú"
        );
    }

    public void StartNewSession()
    {
        if (isChangingScene) return;

        Session.Reset();
        Session.RebuildStatsSystem();

        ChangeScene(
            "LauScene",
            "Comenzando recorrido " + Session.SessionConfig.CurrentLevel
        );
    }

    public void EndSession()
    {
        if (isChangingScene) return;

        Session.Reset();

        ChangeScene(
            MainMenuScene,
            "Volviendo al menú"
        );
    }

    private void ChangeScene(string sceneName, string transitionMessage)
    {
        isChangingScene = true;

        SceneTransitionManager.Instance.TransitionToScene(
            sceneName,
            transitionMessage
        );
    }

    public void FinishSceneChange()
    {
        isChangingScene = false;
    }
}