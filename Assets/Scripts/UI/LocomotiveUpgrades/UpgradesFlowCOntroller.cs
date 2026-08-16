using UnityEngine;
using UnityEngine.UI;

public class ExitStorePanelController : MonoBehaviour
{
    public void ContinueJourney()
    {
        StoreManager.Instance.ExitStore();
    }

}
