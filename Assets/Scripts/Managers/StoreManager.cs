using UnityEngine;

public class StoreManager : MonoBehaviour
{
    private TrainData baseTrainData;
    private TrainData newTrainData;
    private IBuffer buff;
    void Start()
    {
        baseTrainData = GameManager.Instance.TrainData;
    }
    public void AddBuff()
    {
        GameManager.Instance.AddBufferToList(buff);
    }
}
