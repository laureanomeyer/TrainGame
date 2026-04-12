using System;
using System.Collections.Generic;
using UnityEngine;


public class TrainData
{
    private float speed;
    public TrainStats stats;
    private LocomotiveStats baseStats;
    private Transform tailPosition;
    private List<IWagon> wagonsList = new();
    private List<IWagonID> wagonsIDList = new();

    public List<IBuffer> BufferList { get => bufferList; set => bufferList = value; }
    private List<IBuffer> bufferList = new();

    public List<IWagon> WagonList => wagonsList;
    public List<IWagonID> WagonsIDList => wagonsIDList; 
    public Transform TailPosition => tailPosition;
    public float Speed => speed;

    public TrainData(LocomotiveStats stats)
    {
        this.baseStats = stats;
        this.stats = new TrainStats(baseStats.maxHp, baseStats.defense, baseStats.goldMultyplier,
                                    baseStats.damageMultyplier, baseStats.attackSpeed, baseStats.fuelOptimizer, baseStats.baseSpeed);
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
        stats = new TrainStats(
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
            stats += buff.StatsBuff;
        }

        return stats;
    }

    public void ResetValuesToDefault()
    {
        wagonsIDList = new List<IWagonID>();
    }
}

