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
    private List<IBuffer> BufferList = new();

    public List<IWagon> WagonList => wagonsList;
    public Transform TailPosition => tailPosition;
    public float Speed => speed;

    public TrainData(LocomotiveStats stats)
    {
        this.baseStats = stats;
        this.stats = new TrainStats(baseStats.maxFuel, baseStats.maxHp, baseStats.defense, baseStats.goldMultyplier,
                                    baseStats.damageMultyplier, baseStats.attackSpeed, baseStats.fuelOptimizer, baseStats.baseSpeed);
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
    public void UpdateStats()
    {
        foreach (IBuffer buff in BufferList) 
        {
            stats += buff.StatsBuff;

        }

    }
}

