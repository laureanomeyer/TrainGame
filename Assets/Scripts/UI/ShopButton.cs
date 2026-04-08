using UnityEngine;

public class ShopButton : MonoBehaviour
{
    [SerializeField] private string buttonText;

    public string ButtonText => buttonText;

    public void Interact()
    {
        Debug.Log("Interact " + gameObject.name);

        GameManager.Instance.TrainData.AddWagon(new WagonStore(1));

        Debug.Log(GameManager.Instance.TrainData.WagonsIDList.Count);
    }
}