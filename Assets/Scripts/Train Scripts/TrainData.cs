using System;
using System.Collections.Generic;
using UnityEngine;


public class TrainData
{
    private float speed;

    private LocomotiveStatsSO baseStats;
    private TrainStats locomotiveStatsMultiplicator;

    private Transform tailPosition;

    private List<IWagon> wagonsList = new();
    private List<IWagonID> wagonsIDList = new();

    public TrainStats LocomotiveStatsMultiplicator => locomotiveStatsMultiplicator;
    public List<IWagon> WagonList => wagonsList;
    public List<IWagonID> WagonsIDList => wagonsIDList; 
    public Transform TailPosition => tailPosition;
    public float Speed => speed;

    public TrainData(LocomotiveStatsSO stats)
    {
        baseStats = stats;
        locomotiveStatsMultiplicator = new TrainStats(baseStats);
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


    public void ResetValuesToDefault()
    {
        wagonsIDList = new List<IWagonID>();
    }


    public void ChangedWagonIDList(List<IWagonID> wagonIDs)
    {
        wagonsIDList = wagonIDs;
    }
}

