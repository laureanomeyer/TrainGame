using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] public LocomotiveStatsSO baseStats;
    private TrainData trainData;
    private StatSystem statSystem;
    public float runduration;
    private PlayerData playerData;

    public TrainData TrainData => trainData;
    public PlayerData PlayerData => playerData;
    public StatSystem StatsSystem => statSystem;


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
        trainData = new TrainData(baseStats);
        playerData = new PlayerData();
        statSystem = new StatSystem(baseStats, trainData.LocomotiveStatsMultiplicator);
        DontDestroyOnLoad(gameObject);

    }

    public void GoToStore()
    {
        GameEvents.ChangeGold();
        GameEvents.ChangeTrainData();
        statSystem = new StatSystem(baseStats, trainData.LocomotiveStatsMultiplicator);
        SceneManager.LoadScene("Shop");
    }

    public void GoToRun()
    {
        GameEvents.ChangeGold();
        SceneManager.LoadScene("LauScene");
        runduration += 10f;
    }

    public void ResetGame()
    {
        trainData.ResetValuesToDefault();
        playerData.ResetValuesToDefault();
        statSystem = new StatSystem(baseStats, trainData.LocomotiveStatsMultiplicator);
        SceneManager.LoadScene("LauScene");
    }
}
