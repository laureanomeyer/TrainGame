using UnityEngine;
using UnityEngine.UI;

public class EnemyUIHpBar : MonoBehaviour
{
    [SerializeField] private Image hpBarFill;
    [SerializeField] private Canvas canvas;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        transform.forward = mainCamera.transform.forward;
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (hpBarFill == null) return;
        if (maxHealth <= 0) return;

        float amount = Mathf.Clamp01(currentHealth / maxHealth);
        hpBarFill.fillAmount = amount;

        if (canvas != null)
            canvas.enabled = amount > 0f;
    }

    public void Hide()
    {
        if (canvas != null)
            {
                canvas.enabled = false;
            }
    }
}

