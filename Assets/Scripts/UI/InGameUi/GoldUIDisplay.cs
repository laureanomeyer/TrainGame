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

    private void Start()
    {
        colorText = BuyDoneText.color;
    }

    public void UpdatedGold(float gold)
    {
        GoldTextUI.text = gold.ToString();
    }

    public void UpdatedGoldMesseg(float gold)
    {
        GoldTextUI.text = gold.ToString();
        PlayFade();
    }

    public void PlayFade()
    {
        StopAllCoroutines(); 
        StartCoroutine(FadeRoutine());
    }


    IEnumerator FadeRoutine()
    {
        
        colorText.a = minAlpha;
        BuyDoneText.color = colorText;

  
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
