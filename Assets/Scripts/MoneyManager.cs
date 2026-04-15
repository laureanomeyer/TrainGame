using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    private float playerMoney;

    public float PlayerMoney {  get { return playerMoney; } }
    
    void Start()
    {
        playerMoney = GameManager.Instance.PlayerData.GivePlayerGold();
        GameEvents.OnChangeGold += ChangePlayerGoldData;

        Debug.Log("player gold: " + playerMoney);
    }

    private void ChangePlayerGoldData()
    {
        GameManager.Instance.PlayerData.ChangePlayerGold(playerMoney);
    }

    public void ConsumePlayerGold(float amount)
    {
        playerMoney -= amount;
        if (playerMoney < 0)
        {
            playerMoney = 0;
        }

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
