using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Camera gameplayCamera;

    [Header("Animation")]
    [SerializeField] private float popupDuration = 0.65f;
    [SerializeField] private float riseSpeed = 1.5f;
    [SerializeField] private float horizontalDrift = 0.35f;
    [SerializeField] private float spawnHeight = 1.8f;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 40;
    [SerializeField] private int maxSize = 120;

    private IObjectPool<DamagePopup> popupPool;
    private readonly List<DamagePopup> activePopups = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        popupPool = new ObjectPool<DamagePopup>(
            CreatePopup,
            OnGetPopup,
            OnReleasePopup,
            OnDestroyPopup,
            false,
            defaultCapacity,
            maxSize
        );
    }

    private DamagePopup CreatePopup()
    {
        DamagePopup popup = Instantiate(damagePopupPrefab, transform);
        popup.gameObject.SetActive(false);

        return popup;
    }

    private void OnGetPopup(DamagePopup popup)
    {
        popup.gameObject.SetActive(true);
    }

    private void OnReleasePopup(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
    }

    private void OnDestroyPopup(DamagePopup popup)
    {
        Destroy(popup.gameObject);
    }

    public void ShowDamage(float damage, Vector3 enemyPosition)
    {
        if (damagePopupPrefab == null || gameplayCamera == null)
            return;

        DamagePopup popup = popupPool.Get();

        Vector3 spawnPosition = enemyPosition + Vector3.up * spawnHeight;

        popup.Setup(
            damage,
            spawnPosition,
            popupDuration,
            riseSpeed,
            horizontalDrift
        );

        activePopups.Add(popup);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            DamagePopup popup = activePopups[i];

            popup.Tick(deltaTime, gameplayCamera);

            if (!popup.IsFinished)
                continue;

            activePopups.RemoveAt(i);
            popupPool.Release(popup);
        }
    }

    private void OnDestroy()
    {
        popupPool?.Clear();
    }
}