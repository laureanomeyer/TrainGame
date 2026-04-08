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

    bool hello = true;


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
        DontDestroyOnLoad(gameObject);

    }

    private void Update()
    {

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
    public void ResetScene()
    {
        SceneManager.LoadScene("Shop");
        Debug.Log("Scene Reset");
    }
}
