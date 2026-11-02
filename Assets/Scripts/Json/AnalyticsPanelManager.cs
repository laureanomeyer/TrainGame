using UnityEngine;
using UnityEngine.UI;

public class AnalyticsPanelManager : MonoBehaviour
{
    private bool panelActive = false;

    [SerializeField] GameObject optionsPanel;

    [SerializeField] private Button opencloseButton;

    [SerializeField] private Button createUserButton;
    [SerializeField] private Button cleanGlobalDataButton;
    [SerializeField] private Button cleanUsersDataButton;

    void Start()
    {
        AnalyticsManager.Instance.SaveGlobalData();
        AnalyticsManager.Instance.SaveUserData();

        opencloseButton.onClick.AddListener(CallOpenClose);

        createUserButton.onClick.AddListener(CallCreateUser);
        cleanGlobalDataButton.onClick.AddListener(CallClearGlobalData);
        cleanUsersDataButton.onClick.AddListener(CallClearUserData);
    }

    private void CallOpenClose()
    {
        if (panelActive)
        {
            optionsPanel.SetActive(false);
            panelActive = false;
        }
        else
        {
            optionsPanel.SetActive(true);
            panelActive = true;
        }
    }

    private void CallCreateUser()
    {
        AnalyticsManager.Instance.CreateNewUser();
    }

    private void CallClearGlobalData()
    {
        AnalyticsManager.Instance.ClearGlobalData();
    }

    private void CallClearUserData()
    {
        AnalyticsManager.Instance.ClearUserData();
    }
}
