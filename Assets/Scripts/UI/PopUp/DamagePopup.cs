using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro damageText;

    private float timer;
    private float duration;
    private Vector3 velocity;
    private Color textColor;

    public bool IsFinished => timer <= 0f;

    public void Setup(
        float damage,
        Vector3 worldPosition,
        float popupDuration,
        float riseSpeed,
        float horizontalDrift)
    {
        transform.position = worldPosition;

        damageText.text = Mathf.CeilToInt(damage * 10).ToString();

        duration = popupDuration;
        timer = duration;

        velocity = Vector3.up * riseSpeed;

        textColor = damageText.color;
        textColor.a = 1f;
        damageText.color = textColor;
    }

    public void Tick(float deltaTime, Camera camera)
    {
        timer -= deltaTime;

        transform.position += velocity * deltaTime;

        // Siempre mira a la cámara.
        transform.rotation = camera.transform.rotation;

        float alpha = Mathf.Clamp01(timer / duration);

        textColor.a = alpha;
        damageText.color = textColor;
    }
}