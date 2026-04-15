using System;
using System.Collections.Generic;
using UnityEngine;


public class TrainData
{
    private float speed;

    private LocomotiveStatsSO baseStats;
    private TrainStats locomotiveStatsMultiplicator;
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

    public void AddWagonID(IWagonID wagon)
    {
        wagonsIDList.Add(wagon);
    }
    public void RemoveWagonID(IWagonID wagon)
    {
        wagonsIDList.Remove(wagon);
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
            baseStats.maxHp,
            baseStats.defense,
            baseStats.goldMultyplier,
            baseStats.damageMultyplier,
            baseStats.attackSpeed,
            baseStats.fuelOptimizer,
            baseStats.baseSpeed
        );

        
        foreach (IBuffer buff in BufferList)
        {
            TrainStats buffStats = buff.GetStatsBuff(baseStats);
            wagonBuffedStats += buffStats;
        }

        return wagonBuffedStats;
    }

    public void ResetValuesToDefault()
    {
        wagonsIDList = new List<IWagonID>();
        bufferList = new List<IBuffer>();
    }
    public void ResetBuffsList() 
    {
        bufferList = new List<IBuffer>();
    }

    public void ChangedWagonIDList(List<IWagonID> wagonIDs)
    {
        wagonsIDList = wagonIDs;
    }
}

