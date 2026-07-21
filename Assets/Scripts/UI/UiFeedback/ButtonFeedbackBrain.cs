using DG.Tweening;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonFeedbackBrain : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private bool isPressed;
    [SerializeField] private float animTime = 0.25f;

    private void OnEnable()
    {
        button = GetComponent<Button>();
        isPressed = false;
        button.onClick.AddListener(PlayPressSound);
    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlayPressSound);
    }
    public void OnPointerEnter(PointerEventData data)
    {
        if (isPressed) return;
        PlayHoverSound();
        PlayHoverAnimation();
    }
    public void OnPointerExit(PointerEventData data)
    {
        ReverseHoverAnimation(animTime);
    }
    public void OnPointerDown(PointerEventData data) 
    {
        isPressed = true;
        PlayPressedAnimation(0.1f);
    }
    public void OnPointerUp(PointerEventData data)
    {
        isPressed = false;
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
            ReverseHoverAnimation(animTime);
        }
    }

    private void PlayHoverAnimation()
    {
        button.transform.DOScale(1.1f, animTime).SetEase(Ease.OutBounce);
    }
    private void ReverseHoverAnimation(float animTime)
    {
        button.transform.DOScale(1f, animTime).SetEase(Ease.OutBounce);
    }
    private void PlayPressedAnimation(float animTime)
    {
        button.transform.DOScale(0.87f, animTime).SetEase(Ease.InBounce);
    }
    private void ClearAnimations()
    {
        button.transform.DOScale(1f, 0f);
    }
}
