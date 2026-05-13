using TMPro;
using UnityEngine;

public class WagonStoreManager : MonoBehaviour
{
    [Header("Level of progress")]
    [SerializeField] private int level;
    public int Level => level;
    //[SerializeField] public int maxLevel;

    [SerializeField] public TextMeshProUGUI descriptionTextUI;

    public bool TryConsumeGold(float amount)
    {
        return StoreManager.Instance.TrySpendGold(amount);
    }
    public float GetPlayerGold()
    {
        return StoreManager.Instance.GetGold();
    }
}
