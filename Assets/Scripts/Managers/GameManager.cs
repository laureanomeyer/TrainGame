using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] public LocomotiveStats baseStats;
    private TrainData trainData;
    public TrainData TrainData => trainData;

    private PlayerData playerData;
    public PlayerData PlayerData => playerData;



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
            
        }
        trainData = new TrainData(baseStats);
        playerData = new PlayerData();

        DontDestroyOnLoad(gameObject);

    }

    public void AddBufferToList(IBuffer buff)
    {
        trainData.AddToBufferList(buff);
    }
    public void UpdateTrainData()
    {
        //trainData.UpdateStats();
        trainData.stats = trainData.UpdateStats();
    }

    public void GoToStore()
    {
        GameEvents.ChangeGold();
        GameEvents.ChangeTrainData();
        Debug.Log("Oro actual del jugador " + playerData.GivePlayerGold());

        SceneManager.LoadScene("Shop");
    }

    public void GoToRun()
    {
        GameEvents.ChangeGold();
        Debug.Log("Oro actual del jugador " + playerData.GivePlayerGold());
        SceneManager.LoadScene("LauScene");
    }

    public void ResetGame()
    {
        trainData.ResetValuesToDefault();
        playerData.ResetValuesToDefault();

        SceneManager.LoadScene("LauScene");
        Debug.Log("Scene Reset");
    }
}
