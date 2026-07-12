using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    private float playerMoney;
    private PlayerData playerDataRef;
    public float PlayerMoney {  get { return playerMoney; } }

    [SerializeField] private GoldUIDisplay goldDisplay;
    
    void Start()
    {
        playerDataRef = ServiceLocator.Get<PlayerData>();
        playerMoney = playerDataRef.GivePlayerGold();
        GameEvents.OnChangeGold += ChangePlayerGoldData;

        goldDisplay.UpdatedGold(playerMoney);
    }

    private void ChangePlayerGoldData()
    {
        playerDataRef.ChangePlayerGold(playerMoney);
    }

    public void ConsumePlayerGold(float amount)
    {
        playerMoney -= amount;
        if (playerMoney < 0)
        {
            playerMoney = 0;
        }

        goldDisplay.UpdatedGoldMesseg(playerMoney);
    }

    public float ShowPlayerGold()
    {
        return playerMoney;
    }

    private void OnDestroy()
    {
        GameEvents.OnChangeGold -= ChangePlayerGoldData;
    }
}
