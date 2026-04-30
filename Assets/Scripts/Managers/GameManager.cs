using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private LocomotiveStatsSO baseStats;
    public GameSession Session { get; private set; }

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
        Session.SessionConfig.AdvanceRun();
        Session.RebuildStatsSystem();
        SceneManager.LoadScene("Shop");
    }

    public void GoToRun()
    {
        Session.RebuildStatsSystem();
        SceneManager.LoadScene("LauScene");
    }

    public void EndSession()
    {
        Session.Reset();
        SceneManager.LoadScene("MainMenu");
    }
}
