using UnityEngine;
using UnityEngine.UI;

public class ExitStorePanelCOntroller : MonoBehaviour
{
    public void ContinueJourney()
    {
        StoreManager.Instance.ExitStore();
    }

}
