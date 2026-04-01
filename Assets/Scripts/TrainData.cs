using System;
using System.Collections.Generic;
using UnityEngine;


public class TrainData 
{
    private float speed;
    private Transform tailPosition;
    private List<IWagon> wagonsList;

    public List<IWagon> WagonList => wagonsList;
    public Transform TailPosition => tailPosition;
    public float Speed => speed;

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
}

