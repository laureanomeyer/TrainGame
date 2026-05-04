using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

    [SerializeField] private GoldUIDisplay goldDisplay;

    private PlayerData playerData;

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
    }

    private void Start()
    {
        playerData = GameManager.Instance.Session.PlayerData;
        Debug.Log("Oro " + playerData.Gold);
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
}
