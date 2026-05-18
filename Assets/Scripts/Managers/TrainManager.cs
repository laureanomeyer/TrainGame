using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private GameObject goldWagonPrefab;
    [SerializeField] private GameObject locomotivePrefab;

    private List<IWagon> wagonsCreated = new();
    private List<WagonBrain> wagonBrains = new();
    private Transform tail;
    private WagonMovement lastWagon;

    private void Awake()
    {
        BuildTrain();
    }

    private void BuildTrain()
    {
        CreateLocomotive();
        CreateWagons();
        CreateGoldWagon();

        foreach (var brain in wagonBrains) brain.RegisterModifiers();
        foreach (var brain in wagonBrains) brain.StartWagon();
        RunManager.Instance.OnTrainReady(tail, wagonsCreated);
    }

    private void CreateLocomotive()
    {
        GameObject instance = Instantiate(locomotivePrefab);
        var brain = instance.GetComponent<LocomotiveBrain>();
        RunManager.Instance.SetLocoBrain(brain);
        wagonsCreated.Add(brain);
        tail = brain.TailRef;
    }
    private void CreateWagons()
    {
        foreach (var wagonID in GameManager.Instance.Session.TrainData.WagonsIDList)
            CreateWagon(wagonID.Prefab, wagonID);
    }
    public void CreateWagon(GameObject wagonToCreate, IWagonID id)
    {
        GameObject wagonInstance = Instantiate(wagonToCreate, tail.position, tail.rotation);
        WagonMovement wagon = wagonInstance.GetComponent<WagonMovement>();
        WagonBrain wagonBrain = wagonInstance.GetComponent<WagonBrain>();

        wagonsCreated.Add(wagon);
        wagonBrains.Add(wagonBrain);
        wagonBrain.SetWagonID(id);

        AttachWagon(tail, wagon);
    }

    private void CreateGoldWagon()
    {
        GameObject instance = Instantiate(goldWagonPrefab, tail.position, tail.rotation);
        WagonMovement wagon = instance.GetComponent<WagonMovement>();
        WagonBrain brain = instance.GetComponent<WagonBrain>();
        wagonsCreated.Add(wagon);
        wagonBrains.Add(brain);
        AttachWagon(tail, wagon);
    }

    private void AttachWagon(Transform head, WagonMovement wagon)
    {
        wagon.Initialize(head);
        tail = wagon.Tail;
        RunManager.Instance.SetTrainTail(tail);

        if (lastWagon)
        {
            lastWagon.wagonBack.SetActive(false);
        }

        wagon.wagonBack.SetActive(true);
        lastWagon = wagon;
    }
}
