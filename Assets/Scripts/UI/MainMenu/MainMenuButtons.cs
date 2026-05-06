using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] private Button startButton;
    private void Start()
    {
        startButton.onClick.AddListener(GameManager.Instance.StartNewSession);
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveAllListeners();
    }
}
