using System.Collections.Generic;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private GameObject goldWagonPrefab;
    [SerializeField] private GameObject locomotivePrefab;

    private List<IWagon> wagonsCreated = new();
    private List<WagonBrain> wagonBrains = new();
    private Transform tail;
    private WagonBrain lastWagon;

    private StatSystem statSystemRef;
    private TrainData trainDataRef;

    private void Awake()
    {
        trainDataRef = ServiceLocator.Get<TrainData>();
        statSystemRef = ServiceLocator.Get<StatSystem>();
        BuildTrain();
    }

    private void BuildTrain()
    {
        CreateLocomotive();
        CreateWagons();
        CreateGoldWagon();

        foreach (var brain in wagonBrains) brain.RegisterModifiers(statSystemRef);
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
        foreach (var wagonID in trainDataRef.WagonsIDList)
            CreateWagon(wagonID.Prefab, wagonID);
    }
    public void CreateWagon(GameObject wagonToCreate, IWagonID id)
    {
        GameObject wagonInstance = Instantiate(wagonToCreate, tail.position, tail.rotation);
        WagonBrain wagonBrain = wagonInstance.GetComponent<WagonBrain>();

        wagonsCreated.Add(wagonBrain);
        wagonBrains.Add(wagonBrain);
        wagonBrain.SetWagonID(id);

        AttachWagon(tail, wagonBrain);
    }

    private void CreateGoldWagon()
    {
        GameObject instance = Instantiate(goldWagonPrefab, tail.position, tail.rotation);
        WagonBrain brain = instance.GetComponent<WagonBrain>();
        wagonsCreated.Add(brain);
        wagonBrains.Add(brain);
        Debug.Log("tail: " + tail);
        Debug.Log("brain: " + brain);
        AttachWagon(tail, brain);
    }

    private void AttachWagon(Transform head, WagonBrain wagon)
    {

        RunManager.Instance.SetTrainTail(tail);
        if (lastWagon)
        {
            lastWagon.wagonBack.SetActive(false);
        }

        wagon.wagonBack.SetActive(true);
        lastWagon = wagon;
        wagon.InitializeWagonMovement(head);
        tail = wagon.WagonMovement.Tail;
    }
}
