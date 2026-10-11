using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [SerializeField] private GoldUIDisplay goldDisplay;

    [SerializeField] private DisplayTrain displayTrain;

    private PlayerData playerDataRef;
    private TrainData trainDataRef;

    public List<IWagonID> wagonsInTrain;

    void Awake()
    {
        BulletPool bulletPool = ServiceLocator.Get<BulletPool>();
        trainDataRef = ServiceLocator.Get<TrainData>();
        playerDataRef = ServiceLocator.Get<PlayerData>();
        wagonsInTrain = trainDataRef.WagonsIDList;

        displayTrain.Initialize(wagonsInTrain);
        bulletPool.InitializePool();
    }

    private void Start()
    {
        goldDisplay.UpdatedGold(playerDataRef.Gold);
    }


    public bool TrySpendGold(float ammount)
    {
        if (playerDataRef.Gold < ammount) return false;

        playerDataRef.SpendGold(ammount);
        goldDisplay.UpdatedGold(playerDataRef.Gold);
        return true;
    }

    public float GetGold() => playerDataRef.Gold;

    public void AddGold(float ammount)
    {
        playerDataRef.AddPlayerGold(ammount);
        goldDisplay.UpdatedGold(playerDataRef.Gold);
    }

    public void ChangeWagonList()
    {
        trainDataRef.SetNewWagonIDList(displayTrain.ChangeWagonIDList());

/*         foreach (var w in trainDataRef.WagonsIDList)
        {
            Debug.Log("Store Manager: " + w.WagonName);
        } */

        GameManager.Instance.Session.RebuildStatsSystem();
    }

    public void ExitStore()
    {
        ChangeWagonList();
        EventBus.Publish(new OnActivateNonPausableUI(true));
        EventBus.Publish(new OnForceCloseAllUI());
        GameManager.Instance.GoToRun();
    }
}
