using DG.Tweening;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonFeedbackBrain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    [SerializeField] private float animTime = 0.25f;

    private void OnEnable()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayPressSound);
    }
    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData data)
    {
        PlayHoverSound();
        PlayHoverAnimation();
    }
    public void OnPointerExit(PointerEventData data)
    {
        ReverseHoverAnimation();
    }
    private void PlayHoverSound()
    {
        if (button != null) 
        {
            AudioManager.Instance.Play("ButtonHoverSound");
        }
    }
    private void PlayPressSound()
    {
        if (button != null)
        {
            AudioManager.Instance.Play("ButtonPressedSound");
            ReverseHoverAnimation();
        }
    }

    private void PlayHoverAnimation()
    {
        button.transform.DOScale(1.1f, animTime).SetEase(Ease.OutBounce);
    }
    private void ReverseHoverAnimation()
    {
        button.transform.DOScale(1f, animTime).SetEase(Ease.OutBounce);
    }
}
