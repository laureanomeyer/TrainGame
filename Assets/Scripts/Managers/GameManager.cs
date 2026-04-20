using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] public LocomotiveStatsSO baseStats;
    private IState currentState;

    public IState CurrentState => currentState;
    public TrainData TrainData => (currentState as GameplayState)?.TrainData;
    public PlayerData PlayerData => (currentState as GameplayState)?.PlayerData;
    public StatSystem StatsSystem => (currentState as GameplayState)?.StatsSystem;
    public float RunDuration => (currentState as GameplayState).runduration;


    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;    
        }
        DontDestroyOnLoad(gameObject);
        currentState = new GameplayState();
        currentState.Enter(baseStats);
    }

    public void GoToStore()
    {
        (currentState as GameplayState)?.GoToStore();
        SceneManager.LoadScene("Shop");
    }

    public void GoToRun()
    {
        (currentState as GameplayState)?.GoToRun();
        SceneManager.LoadScene("LauScene");
    }

    public void ResetGame()
    {
        (currentState as GameplayState)?.ResetGame();
        SceneManager.LoadScene("LauScene");
    }
}
