using UnityEngine;

public class RepairIconTutorial : MonoBehaviour
{
    private CanvasGroup repairIcon;
    private bool canShow;
    private void Awake()
    {
        if (GameManager.Instance.IsTutorial)
        {
            repairIcon = GetComponent<CanvasGroup>();

            EventBus.Subscribe<OnSetRepairIconEnabledEvent>(SetCanShow);
            EventBus.Subscribe<OnShowRepairIconEvent>(HandleVisibility);
        }
    }

    private void SetCanShow(OnSetRepairIconEnabledEvent ev)
    {
        canShow = ev.Enabled;
    }

    private void HandleVisibility(OnShowRepairIconEvent ev)
    {
        if (ev.Show && canShow)
        {
            repairIcon.alpha = 1f;
        }
        else
        {
            repairIcon.alpha = 0f;
        }
    }
}
