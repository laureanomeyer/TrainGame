using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameplayState: IState
{
    private TrainData trainData;
    private StatSystem statSystem;
    private int currentRun;
    public float runduration;
    private PlayerData playerData;
    private LocomotiveStatsSO baseStats;

    public TrainData TrainData => trainData;
    public PlayerData PlayerData => playerData;
    public StatSystem StatsSystem => statSystem;
    public int CurrentRun => currentRun;
    public void Enter(LocomotiveStatsSO stats) 
    {
        this.baseStats = stats;
        trainData = new TrainData(baseStats);
        playerData = new PlayerData();
        statSystem = new StatSystem(baseStats, trainData.LocomotiveStatsMultiplicator);
        runduration = 30;
        currentRun = 0;
    }
    public void Tick() 
    { 

    }

    public void GoToStore()
    {
        GameEvents.ChangeGold();
        GameEvents.ChangeTrainData();
        statSystem = new StatSystem(baseStats, trainData.LocomotiveStatsMultiplicator);
    }

    public void GoToRun()
    {
        GameEvents.ChangeGold();
        runduration += 15f;
        currentRun += 1;
    }

    public void ResetGame()
    {
        trainData.ResetValuesToDefault();
        playerData.ResetValuesToDefault();
        statSystem = new StatSystem(baseStats, trainData.LocomotiveStatsMultiplicator);
        runduration = 30f;
        currentRun = 0;

        Debug.Log(runduration);
    }

    public void Exit() 
    { 

    } 
}

