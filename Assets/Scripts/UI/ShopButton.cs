using UnityEngine;

public class ShopButton : MonoBehaviour
{
    [SerializeField] private string buttonText;

    public string ButtonText => buttonText;
    public void Interact()
    {
        Debug.Log("Interact " + gameObject.name);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
