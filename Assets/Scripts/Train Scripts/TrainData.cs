using System;
using System.Collections.Generic;
using UnityEngine;


public class TrainData
{
    private float speed;

    private LocomotiveStatsSO baseStats;
    public TrainStats locomotiveStatsMultiplicator;

    private Transform tailPosition;
    private Transform goldBoxPosition;
    private List<IWagonID> wagonsIDList = new();
    public TrainStats LocomotiveStatsMultiplicator => locomotiveStatsMultiplicator;
    
    public List<IWagonID> WagonsIDList => wagonsIDList; 
    public Transform TailPosition => tailPosition;
    public Transform GoldBoxPosition => goldBoxPosition;
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
    public void SetNewWagonIDList(List<IWagonID> list)
    {
        wagonsIDList = list;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
    public void SetTrainTail(Transform tail)
    {
        tailPosition = tail;
    }
    public void SetGoldBox(Transform position)
    {
        goldBoxPosition = position;
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

