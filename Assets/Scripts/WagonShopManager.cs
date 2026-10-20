using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WagonShopManager : MonoBehaviour
{
    [SerializeField] private StoreManager storeManagerRef;

    [Header("Wagon shop sections")]
    [SerializeField] private WagonShopButton[] shopButtons;

    [SerializeField] private DisplayTrain displayTrain;

    [SerializeField] public Button buyButton;
    [SerializeField] public Button rerollButton;
    [SerializeField] public Button closeButton;

    private Tweener shakeTween;
    private Vector2 buyButtonBasePos;

    private int currentLevel;
    public int Level => currentLevel;

    [SerializeField] public TextMeshProUGUI nameTextUI;
    [SerializeField] public TextMeshProUGUI descriptionTextUI;
    [SerializeField] public TextMeshProUGUI priceTextUI;

    private void Start()
    {
        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        currentLevel = sessionConfigRef.CurrentLevel;

        foreach (var button in shopButtons)
        {
            button.Level = currentLevel;
            button.displayTrain = displayTrain;
            button.buyButton = buyButton;

            button.Initialize();
        }
    }

    public bool TryConsumeGold(float amount)
    {
        return storeManagerRef.TrySpendGold(amount);
    }
    public float GetPlayerGold()
    {
        return storeManagerRef.GetGold();
    }

    public void ActivateButtons()
    {
        buyButton.interactable = true;
        rerollButton.interactable = true;
    }

    public void DeactivateButtons()
    {
        buyButton.interactable = false;
        rerollButton.interactable = false;
    }
    public void ShakeBuyButton(float distance, float duration, int vibrato)
    {
        Debug.Log("Shakeo " + distance);
        var rect = (RectTransform)buyButton.transform;

        if (shakeTween.IsActive())
        {
            shakeTween.Kill();
            rect.anchoredPosition = buyButtonBasePos;
        }
        else
        {
            buyButtonBasePos = rect.anchoredPosition;
        }

        shakeTween = rect.DOShakeAnchorPos(duration, new Vector2(distance, 0f), vibrato, 0f, false, true).SetUpdate(true).SetLink(gameObject).OnKill(() => rect.anchoredPosition = buyButtonBasePos);
    }
}
