using System;
using System.Collections.Generic;
using UnityEngine;


public class TrainData
{
    private float speed;

    private LocomotiveStatsSO baseStats;
    public TrainStats locomotiveStatsMultiplicator;
    private TrainStats wagonBuffedStats;

    private Transform tailPosition;

    private List<IWagon> wagonsList = new();
    private List<IWagonID> wagonsIDList = new();
    private List<IBuffer> bufferList = new();

    public TrainStats LocomotiveStatsMultiplicator => locomotiveStatsMultiplicator;
    public TrainStats WagonBuffedStats => wagonBuffedStats;
    public List<IBuffer> BufferList { get => bufferList; set => bufferList = value; }
    public List<IWagon> WagonList => wagonsList;
    public List<IWagonID> WagonsIDList => wagonsIDList; 
    public Transform TailPosition => tailPosition;
    public float Speed => speed;

    public TrainData(LocomotiveStatsSO stats)
    {
        baseStats = stats;
        locomotiveStatsMultiplicator = new TrainStats(baseStats);
        wagonBuffedStats = new TrainStats(baseStats);
    }

    public void AddWagon(IWagonID wagon)
    {
        wagonsIDList.Add(wagon);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
    public void SetTrainTail(Transform tail)
    {
        tailPosition = tail;
    }
    public void SetWagonList(List<IWagon> wagonList)
    {
        this.wagonsList = wagonList;
    }
    public void AddToBufferList(IBuffer buffToAdd)
    {
        BufferList.Add(buffToAdd);
    }

    public TrainStats UpdateStats()
    {
        wagonBuffedStats = new TrainStats(
            locomotiveStatsMultiplicator.trainMaxHp,
            locomotiveStatsMultiplicator.shields,
            locomotiveStatsMultiplicator.goldBonus,
            locomotiveStatsMultiplicator.damageBonus,
            locomotiveStatsMultiplicator.attackSpeed,
            locomotiveStatsMultiplicator.fuelOptimizer,
            locomotiveStatsMultiplicator.baseSpeed
        );

        
        foreach (IBuffer buff in BufferList)
        {
            TrainStats buffStats = buff.GetStatsBuff(baseStats);
            wagonBuffedStats += buffStats;
        }

        Debug.Log("HP FINAL DEL TREN: " + wagonBuffedStats.trainMaxHp);

        return wagonBuffedStats;
    }

    public void ResetValuesToDefault()
    {
        wagonsIDList = new List<IWagonID>();
    }

    public void ChangedWagonIDList(List<IWagonID> wagonIDs)
    {
        wagonsIDList = wagonIDs;
    }
}

