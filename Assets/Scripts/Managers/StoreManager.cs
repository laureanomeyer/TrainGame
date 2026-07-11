using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

    [SerializeField] private GoldUIDisplay goldDisplay;

    [SerializeField] private DisplayTrain displayTrain;

    private PlayerData playerData;

    public List<IWagonID> wagonsInTrain;

    void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        #endregion

        wagonsInTrain = GameManager.Instance.Session._TrainData.WagonsIDList;
    }

    private void Start()
    {
        playerData = GameManager.Instance.Session._PlayerData;
        goldDisplay.UpdatedGold(playerData.Gold);
    }

    public bool TrySpendGold(float ammount)
    {
        if (playerData.Gold < ammount) return false;

        playerData.SpendGold(ammount);
        goldDisplay.UpdatedGold(playerData.Gold);
        return true;
    }

    public float GetGold() => playerData.Gold;

    public void ChangeWagonList()
    {
        GameManager.Instance.Session._TrainData.SetNewWagonIDList(displayTrain.ChangeWagonIDList());
        GameManager.Instance.Session.RebuildStatsSystem();
    }

    public void ExitStore()
    {
        ChangeWagonList();
        GameManager.Instance.GoToRun();
    }
}
