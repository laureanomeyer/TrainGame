using UnityEngine;

public class ExitStorePanelController : MonoBehaviour
{
    [SerializeField] private StoreManager storeManagerRef;

    public void ContinueJourney()
    {
        storeManagerRef.ExitStore();
    }

}
