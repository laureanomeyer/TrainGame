using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [Header("Store assets")]
    [SerializeField] private GoldUIDisplay goldDisplay;
    [SerializeField] private DisplayTrain displayTrain;
    [SerializeField] private RectTransform nameStationTransform;
    [SerializeField] private Transform exitStationTransform;

    [Header("Wagon Store slots")]
    [SerializeField] public Transform wagonSlot1Transform;
    [SerializeField] public Transform wagonSlot2Transform;
    [SerializeField] public Transform wagonSlot3Transform;

    [Header("Desert station")]
    [SerializeField] private int maxLevelWithDesert = 2;
    [SerializeField] GameObject desertStation;
    [SerializeField] private StoreAssetsData desertShopInfo;

    [Header("Grass station")]
    [SerializeField] private int maxLevelWithGrass = 5;
    [SerializeField] GameObject grassStation;
    [SerializeField] private StoreAssetsData grassShopInfo;

    [Header("Cave station")]
    [SerializeField] private int maxLevelWithCave = 11;
    [SerializeField] GameObject caveStation;
    [SerializeField] private StoreAssetsData caveShopInfo;

    private PlayerData playerDataRef;
    private TrainData trainDataRef;

    public List<IWagonID> wagonsInTrain;

    void Awake()
    {
        BulletPool bulletPool = ServiceLocator.Get<BulletPool>();
        trainDataRef = ServiceLocator.Get<TrainData>();
        playerDataRef = ServiceLocator.Get<PlayerData>();
        wagonsInTrain = trainDataRef.WagonsIDList;

        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        string stationName = GameManager.Instance.GetStationNameByLevel(sessionConfigRef.CurrentLevel);

        foreach (var wagon in wagonsInTrain)
        {
            AnalyticsManager.Instance.RegisterWagonOnArrival(stationName, wagon.WagonName);
        }

        int goldInt = (int)playerDataRef.Gold;
        AnalyticsManager.Instance.RegisterGoldInStationOnArrival(stationName, goldInt);

        //Carga de asset de tienda y inicializacion de DisplayTrain

        if(sessionConfigRef.CurrentLevel <= maxLevelWithDesert)
        {
            desertStation.SetActive(true);
            nameStationTransform.localPosition = desertShopInfo.stationNamePosition;
            nameStationTransform.localRotation = Quaternion.Euler(desertShopInfo.stationNameRotation);
            exitStationTransform.position = desertShopInfo.exitStationPosition;

            wagonSlot1Transform.localPosition = desertShopInfo.wagonSlot1Position;
            wagonSlot2Transform.localPosition = desertShopInfo.wagonSlot2Position;
            wagonSlot3Transform.localPosition = desertShopInfo.wagonSlot3Position;

            displayTrain.Initialize(wagonsInTrain, desertShopInfo.floorTile, desertShopInfo.spawnTilePosition, desertShopInfo.locomotivePosition);
        }
        else if (sessionConfigRef.CurrentLevel > maxLevelWithDesert && sessionConfigRef.CurrentLevel <= maxLevelWithGrass)
        {
            grassStation.SetActive(true);
            nameStationTransform.localPosition = grassShopInfo.stationNamePosition;
            nameStationTransform.localRotation = Quaternion.Euler(grassShopInfo.stationNameRotation);
            exitStationTransform.position = grassShopInfo.exitStationPosition;

            wagonSlot1Transform.localPosition = grassShopInfo.wagonSlot1Position;
            wagonSlot2Transform.localPosition = grassShopInfo.wagonSlot2Position;
            wagonSlot3Transform.localPosition = grassShopInfo.wagonSlot3Position;

            displayTrain.Initialize(wagonsInTrain, grassShopInfo.floorTile, grassShopInfo.spawnTilePosition, grassShopInfo.locomotivePosition);
        }
        else if (sessionConfigRef.CurrentLevel > maxLevelWithGrass && sessionConfigRef.CurrentLevel <= maxLevelWithCave)
        {
            caveStation.SetActive(true);
            nameStationTransform.localPosition = caveShopInfo.stationNamePosition;
            nameStationTransform.localRotation = Quaternion.Euler(caveShopInfo.stationNameRotation);
            exitStationTransform.position = caveShopInfo.exitStationPosition;

            wagonSlot1Transform.localPosition = caveShopInfo.wagonSlot1Position;
            wagonSlot2Transform.localPosition = caveShopInfo.wagonSlot2Position;
            wagonSlot3Transform.localPosition = caveShopInfo.wagonSlot3Position;

            displayTrain.Initialize(wagonsInTrain, caveShopInfo.floorTile, caveShopInfo.spawnTilePosition, caveShopInfo.locomotivePosition);
        }

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

        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        string stationName = GameManager.Instance.GetStationNameByLevel(sessionConfigRef.CurrentLevel);

        foreach (var wagon in trainDataRef.WagonsIDList)
        {
            AnalyticsManager.Instance.RegisterWagonOnDeparture(stationName , wagon.WagonName);
        }

        int goldInt = (int)playerDataRef.Gold;
        AnalyticsManager.Instance.RegisterGoldInStationOnDeparture(stationName , goldInt);

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

[System.Serializable]
public class StoreAssetsData
{
    [SerializeField] public GameObject floorTile;

    [SerializeField] public Vector3 spawnTilePosition;
    [SerializeField] public Vector3 locomotivePosition;
    [SerializeField] public Vector3 exitStationPosition;

    [SerializeField] public Vector3 stationNamePosition;
    [SerializeField] public Vector3 stationNameRotation;

    [SerializeField] public Vector3 wagonSlot1Position;
    [SerializeField] public Vector3 wagonSlot2Position;
    [SerializeField] public Vector3 wagonSlot3Position;
}
