using System.Collections;
using TMPro;
using UnityEngine;

public class GoldUIDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI GoldTextUI;
    [SerializeField] private TextMeshProUGUI BuyDoneText;

    [SerializeField] public float speed = 1f;
    public float minAlpha = 0f;
    public float maxAlpha = 1f;

    private UnityEngine.Color colorText;

    public void UpdatedGold(float gold)
    {
        GoldTextUI.text = "Gold: " + gold;
    }

    public void UpdatedGoldMesseg(float gold)
    {
        GoldTextUI.text = "Gold: " + gold;
        PlayFade();
    }

    public void PlayFade()
    {
        StopAllCoroutines(); // evita superposición
        StartCoroutine(FadeRoutine());
    }


    IEnumerator FadeRoutine()
    {
        // Forzar alpha inicial a 0
        colorText.a = minAlpha; // 👈 esto es lo que faltaba
        BuyDoneText.color = colorText;

        // SUBE
        while (colorText.a < maxAlpha)
        {
            colorText.a += speed * Time.deltaTime;
            colorText.a = Mathf.Min(colorText.a, maxAlpha);
            BuyDoneText.color = colorText;
            yield return null;
        }

        // BAJA
        while (colorText.a > minAlpha)
        {
            colorText.a -= speed * Time.deltaTime;
            colorText.a = Mathf.Max(colorText.a, minAlpha);
            BuyDoneText.color = colorText;
            yield return null;
        }
    }
}
