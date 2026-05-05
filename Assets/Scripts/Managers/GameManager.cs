using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private LocomotiveStatsSO baseStats;
    public GameSession Session { get; private set; }

    private bool isChangingScene;

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

        isChangingScene = true;

        Session.SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();

        int currentLevel = Session.SessionConfig.CurrentLevel;

        
        SceneTransitionManager.Instance.TransitionToScene(
            "Shop",
            "Llegando a Estacion " + currentLevel
            );
        
    }

    public void GoToRun()
    {
        if (isChangingScene) return;

        isChangingScene = true;
        Session.RebuildStatsSystem();

        int currentLevel = Session.SessionConfig.CurrentLevel;

        
        SceneTransitionManager.Instance.TransitionToScene(
            "LauScene",
            "Partiendo viaje " + currentLevel
            );
        
    }

    public void EndSession()
    {
        if (isChangingScene) return;

        isChangingScene = true;

        Session.Reset();
        SceneTransitionManager.Instance.TransitionToScene(
            "MainMenu",
            "Volviendo al menu"
            );
    }
    public void FinishSceneChange()
    {
        isChangingScene = false;
    }

}
