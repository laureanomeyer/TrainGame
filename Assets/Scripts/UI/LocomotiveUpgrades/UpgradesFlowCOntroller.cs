using UnityEngine;

public class ExitStorePanelController : MonoBehaviour
{
    public void ContinueJourney()
    {
        StoreManager.Instance.ExitStore();
    }

}
