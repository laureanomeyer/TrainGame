using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private float speed;
    private Transform tailPosition;
    private List<IWagon> wagonList;

    public float Speed => speed;
    public Transform TailPosition => tailPosition;
    public List<IWagon> WagonList => wagonList;

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
            DontDestroyOnLoad(gameObject);
        }
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
        this.wagonList = wagonList;
    }
}
