using UnityEngine;

public class ShopButton : MonoBehaviour
{
    [SerializeField] private string buttonText;
    [SerializeField] private GameObject wagonPrefab;
    public string ButtonText => buttonText;

    public void Interact()
    {
        Debug.Log("Interact " + gameObject.name);

        GameManager.Instance.TrainData.AddWagon(new WagonStore(wagonPrefab));

        Debug.Log(GameManager.Instance.TrainData.WagonsIDList.Count);
    }
}