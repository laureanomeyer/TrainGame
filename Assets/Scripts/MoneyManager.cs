using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    private float playerMoney;

    public float PlayerMoney {  get { return playerMoney; } }
    
    void Start()
    {
        playerMoney = GameManager.Instance.PlayerData.GivePlayerGold();
        GameEvents.OnChangeGold += ChangePlayerGoldData;

        playerMoney += 100f;
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

        Debug.Log("Se consumio oro");
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
