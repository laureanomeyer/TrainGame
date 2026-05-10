using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject textPanel;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text spacialText;
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private TMP_Text stationText;
    [SerializeField] private Image fadeImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        HideAll();
        stationText.text = $"Estación: {GameManager.Instance.Session.SessionConfig.CurrentLevel}";
        spacialText.text = $"Estación {GameManager.Instance.Session.SessionConfig.CurrentLevel}";

        StartCoroutine(StartUi());
    }
    IEnumerator StartUi()
    {
        yield return new WaitForSeconds(fadeDuration);

        yield return FadeText(1f, 0f);

        yield return null;

        yield return FadeImage(1f, 0f);

        fadeImage.gameObject.SetActive(false);
        stationText.gameObject.SetActive(false);
        
    }
    public void ShowText(string message)
    {
        textPanel.SetActive(true);
        buttonsPanel.SetActive(false);
        infoText.text = message;
    }
    private IEnumerator FadeImage(float from, float to)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            SetFadeAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }
        SetFadeAlpha(to);
    }

    private IEnumerator FadeText(float from, float to)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            SetTextAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetTextAlpha(to);
    }

    public void ShowButtons()
    {
        textPanel.SetActive(false);
        buttonsPanel.SetActive(true);
    }

    public void HideAll()
    {
        textPanel.SetActive(false);
        buttonsPanel.SetActive(false);
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = stationText.color;
        color.a = alpha;
        stationText.color = color;
    }
}